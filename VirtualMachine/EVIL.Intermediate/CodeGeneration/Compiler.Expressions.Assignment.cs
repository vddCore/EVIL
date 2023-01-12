using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(AssignmentExpression assignmentExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (assignmentExpression.Left is VariableReferenceExpression varRef)
            {
                if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
                {
                    Visit(assignmentExpression.Left);
                }
                
                Visit(assignmentExpression.Right);

                EmitCompoundAssignment(cg, assignmentExpression.OperationType);

                cg.Emit(OpCode.DUP);
                EmitVariableStore(cg, varRef);
            }
            else if (assignmentExpression.Left is IndexerExpression indExpr)
            {
                if (assignmentExpression.OperationType == AssignmentOperationType.Direct)
                {
                    Visit(indExpr.Indexable);
                    Visit(indExpr.KeyExpression);
                    Visit(assignmentExpression.Right);
                    EmitByteOp(cg, OpCode.STE, 1);
                }
                else
                {
                    Visit(indExpr.Indexable);
                    Visit(indExpr.KeyExpression);
                    Visit(indExpr);
                    Visit(assignmentExpression.Right);
                    EmitCompoundAssignment(cg, assignmentExpression.OperationType);
                    EmitByteOp(cg, OpCode.STE, 1);
                }
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