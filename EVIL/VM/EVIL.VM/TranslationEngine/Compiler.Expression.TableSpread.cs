namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(TableSpreadExpression tableSpreadExpression)
    {
        Visit(tableSpreadExpression.Target);
        Chunk.CodeGenerator.Emit(
            OpCode.TABSPRD,
            (byte)(tableSpreadExpression.IsNilTolerant ? 1 : 0)
        );
    }
}
