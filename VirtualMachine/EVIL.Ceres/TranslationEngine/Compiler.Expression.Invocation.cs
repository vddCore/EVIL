namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

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
