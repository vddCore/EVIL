using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(AssignmentExpression assignmentExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (assignmentExpression.Left is VariableReferenceExpression varRef)
            {
                Visit(assignmentExpression.Left);
                Visit(assignmentExpression.Right);

                EmitCompoundAssignment(cg, assignmentExpression.OperationType);

                cg.Emit(OpCode.DUP);
                EmitVariableStore(cg, varRef);
            }
            else if (assignmentExpression.Left is IndexerExpression indExpr)
            {
                Visit(indExpr.Indexable);
                Visit(indExpr.KeyExpression);
                Visit(assignmentExpression.Right);
                cg.Emit(OpCode.STE, 1);
            }
            else
            {
                throw new CompilerException(
                    $"Cannot assign to '{assignmentExpression.Left.GetType().Name}'",
                    assignmentExpression.Line,
                    assignmentExpression.Column
                );
            }
        }
    }
}