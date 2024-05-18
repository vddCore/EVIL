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

            if (invocationExpression.Parent is RetStatement
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
