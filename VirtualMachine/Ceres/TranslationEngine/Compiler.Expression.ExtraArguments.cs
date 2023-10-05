using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
            Chunk.CodeGenerator.Emit(OpCode.XARGS);
        }
    }
}