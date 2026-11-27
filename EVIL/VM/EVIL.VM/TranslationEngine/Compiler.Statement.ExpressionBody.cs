namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
