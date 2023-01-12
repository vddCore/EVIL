using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(FunctionExpression functionExpression)
        {
            var prevCg = CurrentChunk.GetCodeGenerator();
            var (id, chunk) = CurrentChunk.CreateSubChunk();
            ChunkDefinitionStack.Push(chunk);

            EnterScope();
            {
                var currentCg = CurrentChunk.GetCodeGenerator();

                BuildFunction(
                    currentCg,
                    functionExpression.Parameters,
                    functionExpression.Statements
                );
            }
            LeaveScope();

            ChunkDefinitionStack.Pop();
            prevCg.Emit(OpCode.LDF, id);
        }
    }
}