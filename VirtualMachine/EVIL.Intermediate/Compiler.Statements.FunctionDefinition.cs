using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(FunctionDefinition functionDefinition)
        {
            if (IsLocalScope)
            {
                throw new CompilerException(
                    "Function definition statements are not allowed in a local scope. " +
                    "Perhaps you should use a function expression instead.",
                    functionDefinition.Line,
                    functionDefinition.Column
                );
            }
            
            _executable.DefineGlobal(functionDefinition.Identifier);

            var chunk = new Chunk(functionDefinition.Identifier);
            ChunkDefinitionStack.Push(chunk);

            var cg = CurrentChunk.GetCodeGenerator();
            var paramCount = functionDefinition.Parameters.Count;

            EnterScope();
            {
                var localScope = ScopeStack.Peek();

                for (var i = 0; i < paramCount; i++)
                {
                    var param = functionDefinition.Parameters[i];
                    localScope.DefineParameter(param);

                    cg.Emit(OpCode.STA, paramCount - i - 1);
                }

                foreach (var stmt in functionDefinition.Statements.Statements)
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
            _executable.Chunks.Add(ChunkDefinitionStack.Pop());
        }
    }
}