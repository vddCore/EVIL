namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
