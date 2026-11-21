namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(RetStatement retStatement)
    {
        if (retStatement.Expression != null)
        {
            Visit(retStatement.Expression);

            if (retStatement.Expression is InvocationExpression && Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
            {
                if (opCode == OpCode.TAILINVOKE)
                {
                    return;
                }
            }
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }

        Chunk.CodeGenerator.Emit(OpCode.RET);
    }
}
