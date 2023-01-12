using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Right);
            switch (unaryExpression.Type)
            {
                case UnaryOperationType.Plus:
                {
                    if (operand.Type == DynValueType.Number)
                    {
                        return new DynValue(operand.Number);
                    }

                    throw new RuntimeException(
                        $"Attempt to apply unary + on {operand.Type}.",
                        Environment,
                        unaryExpression.Line
                    );
                }

                case UnaryOperationType.Minus:
                {
                    if (operand.Type == DynValueType.Number)
                    {
                        return new DynValue(-operand.Number);
                    }

                    throw new RuntimeException(
                        $"Attempt to apply unary - on {operand.Type}.",
                        Environment,
                        unaryExpression.Line
                    );
                }

                case UnaryOperationType.Length:
                {
                    switch (operand.Type)
                    {
                        case DynValueType.String:
                            return new DynValue(operand.String.Length);
                        case DynValueType.Table:
                            return new DynValue(operand.Table.Count);
                        default:
                        {
                            throw new RuntimeException(
                                $"Attempt to retrieve the length of {operand.Type}.",
                                Environment,
                                unaryExpression.Line
                            );
                        }
                    }
                }

                case UnaryOperationType.ToString:
                {
                    return operand.AsString();
                }

                case UnaryOperationType.Negation:
                {
                    return new DynValue(!operand.IsTruth);
                }

                case UnaryOperationType.BitwiseNot:
                {
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to negate a {operand.Type}.",
                            Environment,
                            unaryExpression.Line
                        );
                    }

                    return new DynValue(~(int)operand.Number);
                }

                case UnaryOperationType.ToNumber:
                {
                    return operand.AsNumber();
                }

                default:
                {
                    throw new RuntimeException(
                        "Unknown unary operation type.",
                        Environment,
                        unaryExpression.Line
                    );
                }
            }
        }
    }
}