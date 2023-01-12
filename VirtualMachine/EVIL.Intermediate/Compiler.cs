using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Traversal;

namespace EVIL.Intermediate
{
    public partial class Compiler : AstVisitor
    {
        private Executable _executable;

        private int _currentLocalFunctionIndex;

        private Stack<int> LoopContinueLabels { get; } = new();
        private Stack<int> LoopEndLabels { get; } = new();
        
        private Stack<Scope> ScopeStack { get; } = new();
        private Stack<Chunk> ChunkDefinitionStack { get; } = new();

        private Chunk CurrentChunk => ChunkDefinitionStack.Peek();
        private bool IsLocalScope => ScopeStack.Count > 0;

        public Executable Compile(Program program)
        {
            ChunkDefinitionStack.Clear();
            ScopeStack.Clear();

            _executable = new Executable();
            _currentLocalFunctionIndex = 0;

            ChunkDefinitionStack.Push(_executable.MainChunk);
            {
                Visit(program);
            }
            ChunkDefinitionStack.Pop();

            return _executable;
        }

        private int _lastLine = -1;
        public override void Visit(AstNode node)
        {
            if (node.Line != _lastLine)
            {
                if (node.Line == 0)
                {
                    Console.WriteLine(node.GetType().Name);
                }
                Console.WriteLine(node.Line);
                _lastLine = node.Line;
            }
            
            base.Visit(node);
        }

        public override void Visit(Program program)
        {
            foreach (var node in program.Statements)
                Visit(node);

            _executable.MainChunk
                .GetCodeGenerator()
                .Emit(OpCode.HLT);
        }

        public override void Visit(ConditionalExpression conditionalExpression)
        {
        }

        public override void Visit(NumberConstant numberConstant)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            EmitConstantLoadSequence(cg, numberConstant.Value);
        }

        public override void Visit(StringConstant stringConstant)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            EmitConstantLoadSequence(cg, stringConstant.Value);
        }

        public override void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            EmitVariableLoadSequence(cg, variableReferenceExpression);
        }

        public override void Visit(FunctionExpression functionExpression)
        {
        }

        public override void Visit(DoWhileStatement doWhileStatement)
        {
        }

        public override void Visit(WhileStatement whileStatement)
        {
        }

        public override void Visit(BreakStatement breakStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.JUMP, LoopEndLabels.Peek());
        }

        public override void Visit(SkipStatement nextStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.JUMP, LoopContinueLabels.Peek());
        }

        public override void Visit(TableExpression tableExpression)
        {
        }

        public override void Visit(IndexerExpression indexerExpression)
        {
        }

        public override void Visit(UndefStatement undefStatement)
        {
        }

        public override void Visit(EachStatement eachStatement)
        {
        }

        public override void Visit(NameOfExpression nameOfExpression)
        {
        }

        public override void Visit(ExpressionStatement expressionStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            Visit(expressionStatement.Expression);
            cg.Emit(OpCode.POP);
        }

        public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.XARGS);
        }

        private void EnterScope()
        {
            Scope localScope = null;

            if (ScopeStack.Count > 0)
                localScope = ScopeStack.Peek();

            var scope = new Scope(_executable, CurrentChunk, localScope);
            ScopeStack.Push(scope);
        }

        private void LeaveScope()
        {
            if (ScopeStack.Count == 0)
            {
                throw new CompilerException("Cannot leave global scope.");
            }

            ScopeStack.Pop();
        }
    }
}