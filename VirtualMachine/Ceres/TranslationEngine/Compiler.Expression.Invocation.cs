using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(InvocationExpression invocationExpression)
        {
            Visit(invocationExpression.ArgumentList);

            if (invocationExpression.Parent is ReturnStatement
                && invocationExpression.Callee is VariableReferenceExpression varRef
                && varRef.Identifier == Chunk.Name)
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
            }
        }
    }
}