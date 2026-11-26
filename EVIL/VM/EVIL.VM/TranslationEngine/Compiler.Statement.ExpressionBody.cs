namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;

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
