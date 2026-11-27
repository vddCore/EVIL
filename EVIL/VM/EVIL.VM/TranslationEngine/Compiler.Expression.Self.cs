namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

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