using System;
using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UnaryOperationNode unaryOperationNode)
        {
            var operand = Visit(unaryOperationNode.Right);

            switch (unaryOperationNode.Type)
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
                        unaryOperationNode.Line
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
                        unaryOperationNode.Line
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
                                unaryOperationNode.Line
                            );
                        }
                    }
                }

                case UnaryOperationType.ToString:
                    return operand.AsString();

                case UnaryOperationType.NameOf:
                    if (unaryOperationNode.Right is VariableNode variable)
                    {
                        return new DynValue(variable.Identifier);
                    }
                    else if (unaryOperationNode.Right is IndexingNode indexingNode)
                    {
                        return new DynValue(indexingNode.BuildChainStringRepresentation());
                    }

                    throw new RuntimeException(
                        "Attempt to get a name of a non-variable symbol.",
                        Environment,
                        unaryOperationNode.Line
                    );

                case UnaryOperationType.Negation:
                    return new DynValue(!operand.IsTruth);

                case UnaryOperationType.BitwiseNot:
                {
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to negate a {operand.Type}.",
                            Environment,
                            unaryOperationNode.Line
                        );
                    }

                    return new DynValue(~(int)operand.Number);
                }

                case UnaryOperationType.Floor:
                {
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to retrieve floor value of {operand.Type}.",
                            Environment,
                            unaryOperationNode.Line
                        );
                    }

                    return new DynValue(Math.Floor(operand.Number));
                }

                default:
                {
                    throw new RuntimeException(
                        "Unknown unary operation type.",
                        Environment,
                        unaryOperationNode.Line
                    );
                }
            }
        }
    }
}