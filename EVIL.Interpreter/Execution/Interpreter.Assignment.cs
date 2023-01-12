using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(AssignmentNode assignmentNode)
        {
            var left = Visit(assignmentNode.Left);
            var right = Visit(assignmentNode.Right);

            switch (assignmentNode.OperationType)
            {
                case AssignmentOperationType.Direct:
                    break;

                case AssignmentOperationType.Add:
                    right = Addition(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.Subtract:
                    right = Subtraction(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.Multiply:
                    right = Multiplication(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.Divide:
                    right = Division(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.Modulo:
                    right = Modulus(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.BitwiseAnd:
                    right = BitwiseAnd(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.BitwiseOr:
                    right = BitwiseOr(left, right, assignmentNode);
                    break;

                case AssignmentOperationType.BitwiseXor:
                    right = BitwiseXor(left, right, assignmentNode);
                    break;
                
                case AssignmentOperationType.ShiftLeft:
                    right = ShiftLeft(left, right, assignmentNode);
                    break;
                
                case AssignmentOperationType.ShiftRight:
                    right = ShiftRight(left, right, assignmentNode);
                    break;

                default:
                    throw new RuntimeException("Unexpected compound assignment type??", Environment, null);
            }

            left.CopyFrom(right);
            return right;
        }
    }
}