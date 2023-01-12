using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(AssignmentExpression assignmentExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            Visit(assignmentExpression.Right);

            if (assignmentExpression.Left is AssignmentExpression)
            {
                cg.Emit(OpCode.DUP);
                Visit(assignmentExpression.Left);
            }

            var currentLeft = assignmentExpression.Left;
            while (currentLeft is AssignmentExpression ae)
            {
                currentLeft = ae.Left;
                cg.Emit(OpCode.DUP);
            }

            currentLeft = assignmentExpression.Left;
            while (currentLeft is AssignmentExpression ae)
            {
                currentLeft = ae.Left;

                if (ae.Right is VariableReferenceExpression varRef)
                {
                    EmitCompoundAssignmentSequence(cg, varRef, ae.OperationType);
                    cg.Emit(OpCode.DUP);
                    EmitVariableStoreSequence(cg, varRef);
                }
            }

            if (currentLeft is VariableReferenceExpression rootVarRef)
            {
                EmitCompoundAssignmentSequence(cg, rootVarRef, assignmentExpression.OperationType);
                cg.Emit(OpCode.DUP);
                EmitVariableStoreSequence(cg, rootVarRef);
            }
        }
    }
}