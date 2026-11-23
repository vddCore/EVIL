namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

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
