namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

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