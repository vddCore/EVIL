using System;
using System.Collections.Generic;
using System.Linq;
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
        private Script _script = new();

        private readonly Stack<Chunk> _chunks = new();
        private Dictionary<string, List<AttributeProcessor>> _attributeProcessors = new();
        private List<IncludeProcessor> _includeProcessors = new();

        private int _blockDescent;
        private readonly Stack<Loop> _loopDescent = new();

        private readonly List<Scope> _closedScopes = new();
        private Scope RootScope { get; } = Scope.CreateRoot();

        private Scope CurrentScope
        {
            get
            {
                if (_closedScopes.Count == 0)
                    _closedScopes.Add(RootScope);

                return _closedScopes[0];
            }

            set
            {
                if (_closedScopes.Count == 0)
                    _closedScopes.Add(RootScope);

                _closedScopes[0] = value;
            }
        }

        private Chunk Chunk => _chunks.Peek();
        private Loop Loop => _loopDescent.Peek();

        private int Line { get; set; }
        private int Column { get; set; }

        public string CurrentFileName { get; private set; } = string.Empty;

        public CompilerLog Log { get; } = new();

        public Compiler()
        {
            _closedScopes.Add(RootScope);
        }

        public Script Compile(string source, string fileName = "")
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

        public Script Compile(ProgramNode programNode, string fileName = "")
        {
            CurrentFileName = fileName;

            Visit(programNode);
            return _script;
        }

        private void AddCurrentLocationToDebugDatabase()
        {
            if (_blockDescent > 0 && _chunks.Any())
            {
                Chunk.DebugDatabase.AddDebugRecord(
                    Line,
                    Chunk.CodeGenerator.IP
                );
            }
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
            _closedScopes.Insert(0, Scope.CreateRoot());
            {
                action();
            }
            _closedScopes.RemoveAt(0);
        }

        private int InSubChunkDo(Action action)
        {
            var result = Chunk.AllocateSubChunk();
            result.SubChunk.DebugDatabase.DefinedInFile = CurrentFileName;

            _chunks.Push(result.SubChunk);
            {
                action();
            }
            _chunks.Pop();

            return result.Id;
        }

        private void InTopLevelChunkDo(Action action, string? name = null)
        {
            var chunk = new Chunk { Name = name };
            chunk.DebugDatabase.DefinedInFile = CurrentFileName;

            _chunks.Push(chunk);
            {
                action();
            }
            _chunks.Pop();
            _script.Chunks.Add(chunk);
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

            AddCurrentLocationToDebugDatabase();
            base.Visit(node);
            AddCurrentLocationToDebugDatabase();
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

        public void RegisterIncludeProcessor(IncludeProcessor processor)
        {
            if (!_includeProcessors.Contains(processor))
            {
                _includeProcessors.Add(processor);
            }
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