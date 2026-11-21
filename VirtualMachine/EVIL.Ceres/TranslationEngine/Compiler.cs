namespace EVIL.Ceres.TranslationEngine;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Scoping;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar;
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
    private readonly Stack<BlockProtectionInfo> _blockProtectors = [];
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
    private BlockProtectionInfo BlockProtector => _blockProtectors.Peek();        
        
    private int Line { get; set; }
    private int Column { get; set; }

    public string CurrentFileName { get; private set; } = string.Empty;

    private SourceLocation GetLocation(AstNode node) 
        => new(CurrentFileName, node.Line, node.Column);

    private SourceLocation GetLocation(int line, int column) 
        => new(CurrentFileName, line, column);

    public CompilerLog Log { get; } = new();
    public bool OptimizeCodeGeneration { get; set; }

    public Compiler(bool optimizeCodeGeneration = false)
    {
        OptimizeCodeGeneration = optimizeCodeGeneration;
    }

    public Chunk Compile(string source, string fileName = "")
    {
        CurrentFileName = fileName;
        var parser = new Parser(Log, fileName);

        try
        {
            var program = parser.Parse(source);

            if (Log.HasErrors)
            {
                throw new CompilerException(Log);
            }

            return Compile(program, fileName);
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

        return InRootChunkDo(() => Visit(programNode));
    }
        
    private void OnChunkOpCodeEmitted(int ip, OpCode opCode)
    {
        Chunk.DebugDatabase.AddDebugRecord(Line, ip);
    }

    private void OnChunkLabelUpdated(int labelId, int ip)
    {
        if (Chunk.IsLabelTargeted(labelId))
        {
            _isUnreachable = false;
        }
    }

    private Chunk InRootChunkDo(Action action)
    {
        var fileNameBytes = Encoding.UTF8
            .GetBytes(CurrentFileName + Random.Shared.Next());
            
        var hash = SHA1.HashData(fileNameBytes);
        var chunkName = "!_root_chunk";
        var sb = new StringBuilder();
        for (var i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        chunkName += "!" + sb;

        _rootChunk = new Chunk(chunkName);
        _rootChunk.DebugDatabase.DefinedInFile = CurrentFileName;
        _rootChunk.CodeGenerator.OpCodeEmitted = OnChunkOpCodeEmitted;
        _rootChunk.LabelUpdated = OnChunkLabelUpdated;

        _chunks.Push(_rootChunk);
        {
            _isUnreachable = false;
            InNewClosedScopeDo(action);
            FinalizeChunk();
        }
    
        return _chunks.Pop();
    }

    private void InNewLocalScopeDo(Action action)
    {
        CurrentScope = CurrentScope.Descend();
        {
            action();
        }
        CurrentScope = CurrentScope.Parent!;
    }

    private void InNewClosedScopeDo(Action action)
    {
        _closedScopes.Insert(0, Scope.CreateRoot(Chunk));
        {
            action();
        }
        _closedScopes.RemoveAt(0);
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
        {
            action();
        }
        _chunks.Pop();

        _isUnreachable = wasUnreachable;

        return result.Id;
    }

    private int InNamedSubChunkDo(string name, Action action, out bool wasReplaced, out Chunk replacedChunk)
    {
        var result = Chunk.AllocateNamedSubChunk(name, out wasReplaced, out replacedChunk);
        result.SubChunk.DebugDatabase.DefinedInFile = CurrentFileName;
        result.SubChunk.CodeGenerator.OpCodeEmitted = OnChunkOpCodeEmitted;
        result.SubChunk.LabelUpdated = OnChunkLabelUpdated;

        var wasUnreachable = _isUnreachable;
        _isUnreachable = false;

        _chunks.Push(result.SubChunk);
        {
            action();
        }
        _chunks.Pop();

        _isUnreachable = wasUnreachable;

        return result.Id;
    }

    private void InNewLoopDo(Loop.LoopKind kind, Action action, bool needsExtraLabel)
    {
        _loopDescent.Push(new Loop(Chunk, kind, needsExtraLabel));
        {
            action();
        }
        _loopDescent.Pop();
    }

    protected override void Visit(AstNode node)
    {
        Line = node.Line;
        Column = node.Column;

        if (node is Expression expression)
        {
            node = expression.Reduce();
        }

        base.Visit(node);
    }

    public void RegisterAttributeProcessor(string attributeName, AttributeProcessor processor)
    {
        if (!_attributeProcessors.TryGetValue(attributeName, out var list))
        {
            list = [];
            _attributeProcessors.Add(attributeName, list);
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