using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(AssignmentExpression assignmentExpression)
        {
            var right = Visit(assignmentExpression.Right);
            var left = Visit(assignmentExpression.Left);

            switch (assignmentExpression.OperationType)
            {
                case AssignmentOperationType.Direct:
                    break;

                case AssignmentOperationType.Add:
                    right = Addition(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.Subtract:
                    right = Subtraction(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.Multiply:
                    right = Multiplication(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.Divide:
                    right = Division(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.Modulo:
                    right = Modulus(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.BitwiseAnd:
                    right = BitwiseAnd(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.BitwiseOr:
                    right = BitwiseOr(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.BitwiseXor:
                    right = BitwiseXor(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.ShiftLeft:
                    right = ShiftLeft(left, right, assignmentExpression);
                    break;

                case AssignmentOperationType.ShiftRight:
                    right = ShiftRight(left, right, assignmentExpression);
                    break;

                default:
                    throw new RuntimeException("Unexpected compound assignment type??", this, null);
            }

            left.CopyFrom(right);
            return right;
        }
    }
}