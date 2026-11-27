namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
