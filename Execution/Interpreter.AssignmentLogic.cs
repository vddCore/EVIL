using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(AssignmentNode assignmentNode)
        {
            var leftValue = Visit(assignmentNode.Left);
            var rightValue = Visit(assignmentNode.Right);

            switch (assignmentNode.OperationType)
            {
                case AssignmentOperationType.Direct:
                    break;
                
                case AssignmentOperationType.Add:
                    rightValue = new DynValue(leftValue.Number + rightValue.Number);
                    break;
                    
                case AssignmentOperationType.Subtract:
                    rightValue = new DynValue(leftValue.Number - rightValue.Number);
                    break;

                case AssignmentOperationType.Multiply:
                    rightValue = new DynValue(leftValue.Number * rightValue.Number);
                    break;

                case AssignmentOperationType.Divide:
                    if (rightValue.Number == 0)
                        throw new RuntimeException("Cannot divide by zero.", null);

                    rightValue = new DynValue(leftValue.Number / rightValue.Number);
                    break;

                case AssignmentOperationType.Modulo:
                    rightValue = new DynValue(leftValue.Number % rightValue.Number);
                    break;

                case AssignmentOperationType.BitwiseAnd:
                    rightValue = new DynValue((int)leftValue.Number & (int)rightValue.Number);
                    break;

                case AssignmentOperationType.BitwiseOr:
                    rightValue = new DynValue((int)leftValue.Number | (int)rightValue.Number);
                    break;

                case AssignmentOperationType.BitwiseXor:
                    rightValue = new DynValue((int)leftValue.Number ^ (int)rightValue.Number);
                    break;

                default:
                    throw new RuntimeException("Unexpected compound assignment type??", null);
            }

            leftValue.CopyFrom(rightValue);
            return rightValue;
        }
    }
}