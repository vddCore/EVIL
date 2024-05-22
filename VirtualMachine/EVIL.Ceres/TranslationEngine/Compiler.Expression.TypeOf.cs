using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(TypeOfExpression typeOfExpression)
        {
            Visit(typeOfExpression.Target);
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
        }
    }
}