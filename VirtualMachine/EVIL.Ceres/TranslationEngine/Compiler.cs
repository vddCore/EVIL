namespace EVIL.Ceres.TranslationEngine;

using System;
using System.Collections.Generic;
using System.IO;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Scoping;
using EVIL.Diagnostics;
using EVIL.Grammar;
using EVIL.Analysis;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Compiler : AstVisitor
{
    private Chunk _rootChunk = null!;
    private bool _isUnreachable;

    private readonly Stack<Chunk> _chunks = [];
    private readonly Dictionary<string, List<AttributeProcessor>> _attributeProcessors = [];
    private readonly Stack<int> _blockProtectors = [];
    private readonly Stack<Loop> _loopDescent = [];
    private readonly List<Scope> _closedScopes = [];

    private Scope CurrentScope
    {
        get
        {
            if (_closedScopes.Count == 0)
            {
                throw new InvalidOperationException("Internal error: no scopes defined.");
            }

            return _closedScopes[0];
        }

        set
        {
            if (_closedScopes.Count == 0)
            {
                throw new InvalidOperationException("Internal error: no scopes defined");
            }

            _closedScopes[0] = value;
        }
    }

    private Chunk Chunk => _chunks.Peek();
    private Loop Loop => _loopDescent.Peek();
    private int BlockProtector => _blockProtectors.Peek();
        
    private int Line { get; set; }
    private int Column { get; set; }
    
    public string CurrentFileName { get; private set; } = string.Empty;

    public CompilerOptions Options { get; }
    public CompilerLog Log { get; }
    public AnalyzerDriver AnalyzerDriver { get; set; } = AnalyzerDriver.CreateDefault();

    public Compiler(CompilerOptions? compilerOptions = null)
    {
        Options = (compilerOptions ??= CompilerOptions.Default);
        Log = new CompilerLog(this);
    }

    public Chunk Compile(string source, string fileName = "")
    {
        CurrentFileName = fileName;
        
        var parser = new Parser(Log, fileName)
        {
            MaxErrors = Options.MaxErrors
        };

        try
        {
            var program = parser.Parse(source);

            if (Log.HasErrors)
            {
                throw new CompilerException(Log);
            }

            var chunk = Compile(program, fileName);

            if (Log.HasErrors)
            {
                throw new CompilerException(Log);
            }

            return chunk;
        }
        catch (LexerException le)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.LexicalError.Create(
                    new SourceLocation(CurrentFileName, le.Line, le.Column),
                    le.Message
                ),
                innerException: le
            );
        }
        catch (ParserException pe)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.SyntaxError.Create(
                    new SourceLocation(CurrentFileName, pe.Line, pe.Column),
                    pe.Message
                ),
                innerException: pe
            );
        }

        // Dummy return to keep compiler happy.
        return null!;
    }

    public Chunk Compile(ProgramNode programNode, string fileName = "")
    {
        CurrentFileName = fileName;
        _isUnreachable = false;
        _closedScopes.Clear();
        _chunks.Clear();

        if (Options.EnableAnalyzers)
        {
            AnalyzerDriver.Analyze(programNode, CurrentFileName, Log);
        }

        return InRootChunkDo(() => Visit(programNode));
    }
    

    
    private SourceLocation GetLocation(AstNode node) 
        => new(CurrentFileName, node.Line, node.Column);

    private SourceLocation GetLocation(int line, int column) 
        => new(CurrentFileName, line, column);
        
    private void OnChunkOpCodeEmitted(int ip, OpCode opCode)
        => Chunk.DebugDatabase.AddDebugRecord(Line, ip);

    private void OnChunkLabelUpdated(int labelId, int ip)
    {
        if (Chunk.IsLabelTargeted(labelId))
        {
            _isUnreachable = false;
        }
    }

    private Chunk InRootChunkDo(Action action)
    {
        var sourceName = Path.GetFileName(CurrentFileName.Replace('\\', Path.DirectorySeparatorChar));
        var chunkName = string.IsNullOrEmpty(sourceName)
            ? "!_root_chunk"
            : $"!_root_chunk!{sourceName}";

        _rootChunk = new Chunk(chunkName);
        _rootChunk.DebugDatabase.DefinedInFile = CurrentFileName;
        _rootChunk.CodeGenerator.OpCodeEmitted = OnChunkOpCodeEmitted;
        _rootChunk.LabelUpdated = OnChunkLabelUpdated;

        var wasUnreachable = _isUnreachable;
        _chunks.Push(_rootChunk);
        try
        {
            _isUnreachable = false;
            InNewClosedScopeDo(action);
            FinalizeChunk();

            return _rootChunk;
        }
        finally
        {
            _chunks.Pop();
            _isUnreachable = wasUnreachable;
        }
    }

    private void InNewLocalScopeDo(Action action)
    {
        var previousScope = CurrentScope;
        CurrentScope = previousScope.Descend();
        try
        {
            action();
        }
        finally
        {
            CurrentScope = previousScope;
        }
    }

    private void InNewClosedScopeDo(Action action)
    {
        _closedScopes.Insert(0, Scope.CreateRoot(Chunk));
        try
        {
            action();
        }
        finally
        {
            _closedScopes.RemoveAt(0);
        }
    }

    private int InAnonymousSubChunkDo(Action action)
    {
        var result = Chunk.AllocateAnonymousSubChunk();
        result.SubChunk.DebugDatabase.DefinedInFile = CurrentFileName;
        result.SubChunk.CodeGenerator.OpCodeEmitted = OnChunkOpCodeEmitted;
        result.SubChunk.LabelUpdated = OnChunkLabelUpdated;

        var wasUnreachable = _isUnreachable;
        _isUnreachable = false;

        _chunks.Push(result.SubChunk);
        try
        {
            action();
        }
        finally
        {
            _chunks.Pop();
            _isUnreachable = wasUnreachable;
        }

        return result.Id;
    }

    private int InNamedSubChunkDo(
        string name,
        Action action,
        out bool wasReplaced,
        out Chunk replacedChunk,
        bool allocateNewVersionOnRedefinition = false
    )
    {
        var result = Chunk.AllocateNamedSubChunk(
            name,
            allocateNewVersionOnRedefinition,
            out wasReplaced,
            out replacedChunk
        );
        result.SubChunk.DebugDatabase.DefinedInFile = CurrentFileName;
        result.SubChunk.CodeGenerator.OpCodeEmitted = OnChunkOpCodeEmitted;
        result.SubChunk.LabelUpdated = OnChunkLabelUpdated;

        var wasUnreachable = _isUnreachable;
        _isUnreachable = false;

        _chunks.Push(result.SubChunk);
        try
        {
            action();
        }
        finally
        {
            _chunks.Pop();
            _isUnreachable = wasUnreachable;
        }

        return result.Id;
    }

    private void InNewLoopDo(Loop.LoopKind kind, Action action, bool needsExtraLabel)
    {
        _loopDescent.Push(new Loop(Chunk, kind, needsExtraLabel));
        try
        {
            action();
        }
        finally
        {
            _loopDescent.Pop();
        }
    }
    
    private void MarkStatementBoundary(Statement stmt)
    {
        if (_chunks.Count > 0)
        {
            var currentIP = Chunk.CodeGenerator.IP;
            Chunk.DebugDatabase.AddStatementRecord(
                currentIP,
                stmt.Line,
                stmt.Column,
                stmt.Line,
                stmt.Column,
                isStepBoundary: true
            );
        }
    }

    public void RegisterAttributeProcessor(string attributeName, AttributeProcessor processor)
    {
        if (!_attributeProcessors.TryGetValue(attributeName, out var list))
        {
            list = [];
            
            _attributeProcessors.Add(
                attributeName, 
                list
            );
        }

        list.Add(processor);
    }

    private DynamicValue ExtractConstantValueFrom(AstNode valueNode)
    {
        switch (valueNode)
        {
            case BooleanConstant boolConst:
                return boolConst.Value;
            
            case NilConstant:
                return DynamicValue.Nil;
            
            case NumberConstant numConst:
                return numConst.Value;
            
            case StringConstant stringConst:
                return stringConst.Value;
            
            case TypeCodeConstant typeCodeConst:
                return typeCodeConst.Value;

            default:
            {
                Log.TerminateWithInternalFailure(
                    CompilerDiagnostics.InternalCompilerError.Create(
                        GetLocation(valueNode),
                        $"Unexpected constant value node type '{valueNode.GetType().FullName}'."
                    ),
                    dummyReturn: DynamicValue.Zero
                );

                // Dummy return to keep compiler happy.
                return DynamicValue.Zero;
            }
        }
    }
}