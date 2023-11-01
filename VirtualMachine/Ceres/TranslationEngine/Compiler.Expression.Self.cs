using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(SelfExpression selfExpression)
        {
            if (!Chunk.IsSelfAware)
            {
                Log.TerminateWithFatal(
                    "Attempt to use `self' in a self-unaware function.",
                    CurrentFileName,
                    EvilMessageCode.SelfUsedInSelfUnawareFunction,
                    Line,
                    Column
                );
            }
            
            Chunk.CodeGenerator.Emit(OpCode.GETARG, 0);
        }
    }
}