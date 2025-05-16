namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(InvocationExpression invocationExpression)
    {
        Visit(invocationExpression.ArgumentList);

        if (invocationExpression is { Parent: RetStatement, Callee: SymbolReferenceExpression varRef }
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