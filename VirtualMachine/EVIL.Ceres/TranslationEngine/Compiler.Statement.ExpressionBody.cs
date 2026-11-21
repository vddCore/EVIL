namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ExpressionBodyStatement expressionBodyStatement)
    {
        Visit(expressionBodyStatement.Expression);

        if (expressionBodyStatement.Expression is InvocationExpression && Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
        {
            if (opCode == OpCode.TAILINVOKE)
            {
                _isUnreachable = true;
                return;
            }
        }

        Chunk.CodeGenerator.Emit(OpCode.RET);
        _isUnreachable = true;
    }
}
