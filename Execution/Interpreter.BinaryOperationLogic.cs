using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BinaryOperationNode binaryOperationNode)
        {
            var left = Visit(binaryOperationNode.LeftOperand);
            var right = Visit(binaryOperationNode.RightOperand);

            switch (binaryOperationNode.Type)
            {
                case BinaryOperationType.Plus:
                    if (left.Type == DynValueType.String)
                        return new DynValue(left.String + right.String);
                    else
                        return new DynValue(left.Number + right.Number);

                case BinaryOperationType.Minus:
                    return new DynValue(left.Number - right.Number);

                case BinaryOperationType.Multiply:
                    return new DynValue(left.Number * right.Number);

                case BinaryOperationType.Divide:
                {
                    if (right.Number == 0)
                        throw new RuntimeException("Trying to divide by zero.", binaryOperationNode.Line);
                    
                    return new DynValue(left.Number / right.Number);
                }

                case BinaryOperationType.ShiftLeft:
                {
                    if (left.Type != DynValueType.Number || right.Type != DynValueType.Number)
                        throw new RuntimeException("Attempt of left shift operation on a non-numerical value.", binaryOperationNode.Line);

                    if (left.Number % 1 != 0 || right.Number % 1 != 0)
                        throw new RuntimeException("'<<' operation is only allowed on integers.", binaryOperationNode.Line);

                    return new DynValue((int)left.Number << (int)right.Number);
                }
                
                case BinaryOperationType.ShiftRight:
                {
                    if (left.Type != DynValueType.Number || right.Type != DynValueType.Number)

                        throw new RuntimeException("Attempt of right shift operation on a non-numerical value.", binaryOperationNode.Line);

                    if (left.Number % 1 != 0 || right.Number % 1 != 0)
                        throw new RuntimeException("'>>' operation is only allowed on integers.", binaryOperationNode.Line);

                    return new DynValue((int)left.Number >> (int)right.Number);
                }
                
                case BinaryOperationType.BitwiseAnd:
                {
                    if (left.Type != DynValueType.Number || right.Type != DynValueType.Number)
                        throw new RuntimeException("Attempt of a bitwise AND operation on a non-numerical value.", binaryOperationNode.Line);

                    return new DynValue((int)left.Number & (int)right.Number);
                }
                
                case BinaryOperationType.BitwiseOr:
                {
                    if (left.Type != DynValueType.Number || right.Type != DynValueType.Number)
                        throw new RuntimeException("Attempt of a bitwise OR operation on a non-numerical value.", binaryOperationNode.Line);

                    return new DynValue((int)left.Number | (int)right.Number);
                }
                
                case BinaryOperationType.BitwiseXor:
                {
                    if (left.Type != DynValueType.Number || right.Type != DynValueType.Number)
                        throw new RuntimeException("Attempt of a bitwise XOR operation on a non-numerical value.", binaryOperationNode.Line);

                    return new DynValue((int)left.Number ^ (int)right.Number);
                }

                case BinaryOperationType.Modulo:
                    return new DynValue(left.Number % right.Number);

                case BinaryOperationType.Less:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number < right.Number)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else if (left.Type == DynValueType.String)
                    {
                        if (left.String.Length < right.String.Length)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'<' operator is only allowed on numbers and strings.", binaryOperationNode.Line);

                case BinaryOperationType.Greater:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number > right.Number)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else if (left.Type == DynValueType.String)
                    {
                        if (left.String.Length > right.String.Length)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'>' operator is only allowed on numbers and strings.", binaryOperationNode.Line);

                case BinaryOperationType.LessOrEqual:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number <= right.Number)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else if (left.Type == DynValueType.String)
                    {
                        if (left.String.Length <= right.String.Length)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'<=' operator is only allowed on numbers and strings.", binaryOperationNode.Line);

                case BinaryOperationType.GreaterOrEqual:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number >= right.Number)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else if (left.Type == DynValueType.String)
                    {
                        if (left.String.Length >= right.String.Length)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'>=' operator is only allowed on numbers and strings.", binaryOperationNode.Line);

                case BinaryOperationType.NotEqual:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number != right.Number)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else if (left.Type == DynValueType.String)
                    {
                        if (left.String.Length != right.String.Length && right.String != left.String)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'!=' operator is only allowed on numbers and strings.", binaryOperationNode.Line);

                case BinaryOperationType.Equal:
                    if (left.Type != right.Type)
                        throw new RuntimeException("Only values of the same type can be compared.", binaryOperationNode.Line);

                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number == right.Number)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else if (left.Type == DynValueType.String)
                    {
                        if (left.String.Length == right.String.Length && right.String == left.String)
                            return new DynValue(1);

                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'==' operator is only allowed on numbers and strings.", binaryOperationNode.Line);

                case BinaryOperationType.And:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number != 0 && right.Number != 0)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'and' operator is only allowed on numbers.", binaryOperationNode.Line);

                case BinaryOperationType.Or:
                    if (left.Type == DynValueType.Number)
                    {
                        if (left.Number != 0 || right.Number != 0)
                            return new DynValue(1);
                        else return new DynValue(0);
                    }
                    else throw new RuntimeException("'or' operator is only allowed on numbers.", binaryOperationNode.Line);


                default: throw new RuntimeException("Unknown binary operation.", binaryOperationNode.Line);
            }
        }

    }
}
