﻿using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Traversal;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler : AstVisitor
    {
        internal int CurrentLine = -1;
        internal int CurrentColumn = -1;

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
            _executable.Chunks.Add(new Chunk("!root"));
            {
                ChunkDefinitionStack.Push(_executable.RootChunk);
                {
                    Visit(program);
                }
                ChunkDefinitionStack.Pop();
            }
            return _executable;
        }

        public override void Visit(AstNode node)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var line = CurrentLine;
            var col = CurrentColumn;
            
            CurrentLine = node.Line;
            CurrentColumn = node.Column;

            base.Visit(node);
            
            CurrentChunk.DebugInfo.Add(
                new DebugEntry(line, col, cg.IP)
            );
        }

        internal void DefineGlobal(string name)
        {
            if (!_executable.Globals.Contains(name))
                _executable.Globals.Add(name);
        }

        internal bool IsGlobalDefined(string name)
        {
            return _executable.Globals.Contains(name);
        }

        private void BuildFunction(CodeGenerator cg, List<string> parameters, BlockStatement block)
        {
            var paramCount = parameters.Count;
            var localScope = ScopeStack.Peek();

            if (parameters.Count > 255)
            {
                throw new CompilerException(
                    "Upper parameter limit (255) for a function definition has been reached.\n" +
                    "Perhaps it is time to simplify your function.",
                    CurrentLine,
                    CurrentColumn
                );
            }

            for (var i = 0; i < paramCount; i++)
            {
                var param = parameters[i];
                localScope.DefineParameter(param);

                EmitByteOp(cg, OpCode.STA, (byte)(paramCount - i - 1));
            }

            foreach (var stmt in block.Statements)
            {
                Visit(stmt);
            }

            if (CurrentChunk.Instructions.Count == 0 ||
                CurrentChunk.Instructions[^1] != (byte)OpCode.RETN
                && CurrentChunk.Instructions[^1] != (byte)OpCode.TCALL)
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

            var scope = new Scope(this, _executable, CurrentChunk, localScope);
            ScopeStack.Push(scope);
        }

        private void LeaveScope()
        {
            ScopeStack.Pop();
        }
    }
}