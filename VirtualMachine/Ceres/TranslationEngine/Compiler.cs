using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.TranslationEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.Parsing;
using EVIL.Grammar.Traversal;
using EVIL.Lexical;
using static Ceres.TranslationEngine.Loop;

namespace Ceres.TranslationEngine
{
    public partial class Compiler : AstVisitor
    {
        private Chunk _rootChunk = null!;

        private readonly Stack<Chunk> _chunks = new();
        private Dictionary<string, List<AttributeProcessor>> _attributeProcessors = new();

        private int _blockDescent;
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

        private int Line { get; set; }
        private int Column { get; set; }

        private Stack<(int Line, int Column)> LocationStack { get; } = new();

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
                    line: Line,
                    column: Column,
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

        private Chunk InRootChunkDo(Action action)
        {
            _rootChunk = new Chunk("!_root_chunk");
            _rootChunk.DebugDatabase.DefinedInFile = CurrentFileName;

            _chunks.Push(_rootChunk);
            {
                InNewClosedScopeDo(action);
                FinalizeChunk();
            }

            _rootChunk.Name += "!";
            var source = Encoding.UTF8
                .GetBytes(CurrentFileName)
                .Concat(_rootChunk.Code)
                .ToArray();
            
            var hash = SHA1.HashData(source);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            _rootChunk.Name += sb.ToString();
            
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
            _closedScopes.Insert(0, Scope.CreateRoot(Chunk.Name));
            {
                action();
            }
            _closedScopes.RemoveAt(0);
        }

        private int InAnonymousSubChunkDo(Action action)
        {
            var result = Chunk.AllocateAnonymousSubChunk();
            result.SubChunk.DebugDatabase.DefinedInFile = CurrentFileName;

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

            _chunks.Push(result.SubChunk);
            {
                action();
            }
            _chunks.Pop();

            return result.Id;
        }

        private void InNewLoopDo(LoopKind kind, Action action, bool needsExtraLabel)
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

            LocationStack.Push((Line, Column));

            if (node is Expression expression && OptimizeCodeGeneration)
            {
                node = expression.Reduce();
            }

            if (_blockDescent > 0 || _chunks.Count == 1)
            {
                Chunk.DebugDatabase.AddDebugRecord(Line, Chunk.CodeGenerator.LastOpCodeIP);
            }

            base.Visit(node);
            
            Chunk.DebugDatabase.AddDebugRecord(Line, Chunk.CodeGenerator.LastOpCodeIP);
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
}