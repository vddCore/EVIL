using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(FunctionDefinition functionDefinition)
        {
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
            LeaveScope();
            _executable.Chunks.Add(ChunkDefinitionStack.Pop());
        }
    }
}