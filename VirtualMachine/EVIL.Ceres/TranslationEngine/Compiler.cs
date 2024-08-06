namespace EVIL.Ceres.TranslationEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Scoping;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.Parsing;
using EVIL.Grammar.Traversal;
using EVIL.Lexical;

public partial class Compiler : AstVisitor
{
    private Chunk _rootChunk = null!;

    private readonly Stack<Chunk> _chunks = new();
    private Dictionary<string, List<AttributeProcessor>> _attributeProcessors = new();

    private readonly Stack<BlockProtectionInfo> _blockProtectors = new();
    private readonly Stack<Loop> _loopDescent = new();
    private readonly List<Scope> _closedScopes = new();

    private Scope CurrentScope
    {
        get
        {
            if (!_closedScopes.Any())
            {
                throw new InvalidOperationException("Internal error: no scopes defined.");
            }

            return _closedScopes[0];
        }

        set
        {
            if (!_closedScopes.Any())
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

    public CompilerLog Log { get; } = new();
    public bool OptimizeCodeGeneration { get; set; }

    public Compiler(bool optimizeCodeGeneration = false)
    {
        OptimizeCodeGeneration = optimizeCodeGeneration;
    }

    public Chunk Compile(string source, string fileName = "")
    {
        CurrentFileName = fileName;
        var parser = new Parser();

        try
        {
            var program = parser.Parse(source);
            return Compile(program, fileName);
        }
        catch (LexerException le)
        {
            Log.TerminateWithFatal(
                le.Message,
                CurrentFileName,
                EvilMessageCode.LexerError,
                line: le.Line,
                column: le.Column,
                innerException: le
            );
        }
        catch (ParserException pe)
        {
            Log.TerminateWithFatal(
                pe.Message,
                CurrentFileName,
                EvilMessageCode.ParserError,
                line: pe.Line,
                column: pe.Column,
                innerException: pe
            );
        }

        // Dummy return to keep compiler happy.
        return null!;
    }

    public Chunk Compile(ProgramNode programNode, string fileName = "")
    {
        CurrentFileName = fileName;
        _closedScopes.Clear();
        _chunks.Clear();

        return InRootChunkDo(() => Visit(programNode));
    }
        
    private void OnChunkOpCodeEmitted(int ip, OpCode opCode)
    {
        Chunk.DebugDatabase.AddDebugRecord(Line, ip);
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

        _chunks.Push(_rootChunk);
        {
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

        _chunks.Push(result.SubChunk);
        {
            action();
        }
        _chunks.Pop();

        return result.Id;
    }

    private int InNamedSubChunkDo(string name, Action action, out bool wasReplaced, out Chunk replacedChunk)
    {
        var result = Chunk.AllocateNamedSubChunk(name, out wasReplaced, out replacedChunk);
        result.SubChunk.DebugDatabase.DefinedInFile = CurrentFileName;
        result.SubChunk.CodeGenerator.OpCodeEmitted = OnChunkOpCodeEmitted;

        _chunks.Push(result.SubChunk);
        {
            action();
        }
        _chunks.Pop();

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

    public override void Visit(AstNode node)
    {
        Line = node.Line;
        Column = node.Column;

        if (node is Expression expression && OptimizeCodeGeneration)
        {
            node = expression.Reduce();
        }

        base.Visit(node);
    }

    public void RegisterAttributeProcessor(string attributeName, AttributeProcessor processor)
    {
        if (!_attributeProcessors.TryGetValue(attributeName, out var list))
        {
            list = new List<AttributeProcessor>();
            _attributeProcessors.Add(attributeName, list);
        }

        list.Add(processor);
    }

    private DynamicValue ExtractConstantValueFrom(AstNode valueNode)
    {
        if (valueNode is BooleanConstant boolConst)
        {
            return boolConst.Value;
        }
        else if (valueNode is NilConstant)
        {
            return DynamicValue.Nil;
        }
        else if (valueNode is NumberConstant numConst)
        {
            return numConst.Value;
        }
        else if (valueNode is StringConstant stringConst)
        {
            return stringConst.Value;
        }
        else if (valueNode is TypeCodeConstant typeCodeConst)
        {
            return typeCodeConst.Value;
        }
        else
        {
            Log.TerminateWithInternalFailure(
                $"Unexpected constant value node type '{valueNode.GetType().FullName}'.",
                CurrentFileName,
                line: valueNode.Line,
                column: valueNode.Column,
                dummyReturn: DynamicValue.Zero
            );

            // Dummy return to keep compiler happy.
            return DynamicValue.Zero;
        }
    }
}