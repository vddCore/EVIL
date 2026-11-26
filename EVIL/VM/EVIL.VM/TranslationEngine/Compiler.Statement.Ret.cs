namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;

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
                    _isUnreachable = true;
                    return;
                }
            }
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }

        Chunk.CodeGenerator.Emit(OpCode.RET);
        _isUnreachable = true;
    }
}
