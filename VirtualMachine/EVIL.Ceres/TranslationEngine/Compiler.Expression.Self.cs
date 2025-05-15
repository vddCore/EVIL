namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SelfExpression selfExpression)
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