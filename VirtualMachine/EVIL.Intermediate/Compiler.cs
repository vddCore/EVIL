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

        private int _lastLine = -1;
        public override void Visit(AstNode node)
        {
            Console.WriteLine($"{node.Line}:{node.Column}");

            if (node.Line != _lastLine)
            {
                // if (node.Line == 0)
                // {
                //     //Console.WriteLine(node.GetType().Name);
                // }

                _lastLine = node.Line;
            }
            
            base.Visit(node);
        }
        
        public override void Visit(FunctionExpression functionExpression)
        {
            var prevCg = CurrentChunk.GetCodeGenerator();
            var (id, chunk) = _executable.CreateAnonymousChunk();

            ChunkDefinitionStack.Push(chunk);

            BuildFunction(
                CurrentChunk.GetCodeGenerator(), 
                functionExpression.Parameters,
                functionExpression.Statements
            );
            
            _executable.Chunks.Add(ChunkDefinitionStack.Pop());
            prevCg.Emit(OpCode.LDF, id);
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

            EnterScope();
            {
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
            }

            if (CurrentChunk.Instructions.Count == 0 || 
                CurrentChunk.Instructions[^1] != (byte)OpCode.RETN)
            {
                EmitConstantLoad(cg, 0);
                cg.Emit(OpCode.RETN);
            }
            LeaveScope();
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