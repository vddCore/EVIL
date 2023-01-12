using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(IndexerExpression indexerExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            Visit(indexerExpression.Indexable);
            Visit(indexerExpression.KeyExpression);
            cg.Emit(OpCode.INDEX);
        }
    }
}