namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    private static bool IsInTailPosition(AstNode node)
    {
        while (node.Parent != null)
        {
            if (node.Parent is RetStatement or ExpressionBodyStatement)
            {
                return true;
            }

            if (node.Parent is ConditionalExpression cond && (cond.TrueExpression == node || cond.FalseExpression == node))
            {
                node = cond;
                continue;
            }

            return false;
        }

        return false;
    }

    protected override void Visit(InvocationExpression invocationExpression)
    {
        if (invocationExpression.IsConditional)
        {
            var callLabel = Chunk.CreateLabel();
            var endLabel = Chunk.CreateLabel();
            var tempCallee = Chunk.AllocateLocal();

            Visit(invocationExpression.Callee);
            Chunk.CodeGenerator.Emit(OpCode.DUP);
            Chunk.CodeGenerator.Emit(OpCode.VJMP, callLabel);
            // Callee is nil: DUP left nil on stack. Jump directly to end!
            Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);

            Chunk.UpdateLabel(callLabel, Chunk.CodeGenerator.IP);
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempCallee);

            Visit(invocationExpression.ArgumentList);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempCallee);

            Chunk.CodeGenerator.Emit(
                OpCode.INVOKE,
                invocationExpression.ArgumentList.Arguments.Count
            );

            Chunk.CodeGenerator.Emit(
                invocationExpression.ArgumentList.IsVariadic
                    ? (byte)1
                    : (byte)0
            );

            Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
        }
        else
        {
            Visit(invocationExpression.ArgumentList);

            if (IsInTailPosition(invocationExpression)
                && invocationExpression.Callee is SymbolReferenceExpression varRef
                && varRef.Identifier == Chunk.Name
                && !invocationExpression.ArgumentList.IsVariadic)
            {
                Chunk.CodeGenerator.Emit(OpCode.TAILINVOKE);
            }
            else
            {
                Visit(invocationExpression.Callee);
                Chunk.CodeGenerator.Emit(
                    OpCode.INVOKE,
                    invocationExpression.ArgumentList.Arguments.Count
                );
                    
                Chunk.CodeGenerator.Emit(
                    invocationExpression.ArgumentList.IsVariadic
                        ? (byte)1
                        : (byte)0
                );
            }
        }
    }
}
