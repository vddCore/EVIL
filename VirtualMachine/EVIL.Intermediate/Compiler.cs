using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Traversal;

namespace EVIL.Intermediate
{
    public partial class Compiler : AstVisitor
    {
        private int _currentLine = -1;
        private int _currentColumn = -1;
        
        private Executable _executable;

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

            ChunkDefinitionStack.Push(_executable.RootChunk);
            {
                Visit(program);
            }
            ChunkDefinitionStack.Pop();

            return _executable;
        }

        public override void Visit(AstNode node)
        {
            _currentLine = node.Line;
            _currentColumn = node.Column;
            
            base.Visit(node);
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

        private void BuildFunction(CodeGenerator cg, List<string> parameters, BlockStatement block)
        {
            var paramCount = parameters.Count;
            var localScope = ScopeStack.Peek();

            for (var i = 0; i < paramCount; i++)
            {
                var param = parameters[i];
                localScope.DefineParameter(param);

                cg.Emit(OpCode.STA, paramCount - i - 1);
            }

            foreach (var stmt in block.Statements)
            {
                Visit(stmt);
            }

            if (CurrentChunk.Instructions.Count == 0 ||
                CurrentChunk.Instructions[^1] != (byte)OpCode.RETN)
            {
                EmitConstantLoad(cg, 0);
                cg.Emit(OpCode.RETN);
            }
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
            ScopeStack.Pop();
        }
    }
}