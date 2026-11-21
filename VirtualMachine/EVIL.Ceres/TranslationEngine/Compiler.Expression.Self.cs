namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SelfExpression selfExpression)
    {
        if (!Chunk.IsSelfAware)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.SelfUsedInSelfUnawareFunction.Create(
                    GetLocation(Line, Column)
                )
            );
        }
            
        Chunk.CodeGenerator.Emit(OpCode.GETARG, 0);
    }
}