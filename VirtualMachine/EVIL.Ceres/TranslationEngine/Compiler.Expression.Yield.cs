using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(YieldExpression yieldExpression)
        {
            Visit(yieldExpression.ArgumentList);
            Visit(yieldExpression.Target);

            Chunk.CodeGenerator.Emit(
                OpCode.YIELD,
                yieldExpression.ArgumentList.Arguments.Count
            );

            Chunk.CodeGenerator.Emit(OpCode.YRET);
        }
    }
}