using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ErrorExpression errorExpression)
        {
            Visit(errorExpression.UserDataTable);
            Chunk.CodeGenerator.Emit(OpCode.ERRNEW);
        }
    }
}