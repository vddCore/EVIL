using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UnaryOperationNode unaryOperationNode)
        {
            var operand = Visit(unaryOperationNode.Operand);
            
            switch (unaryOperationNode.Type)
            {
                case UnaryOperationType.Plus:
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to apply unary + on {operand.Type}.",
                            Environment,
                            unaryOperationNode.Line
                        );
                    }
                    
                    return new DynValue(operand.Number);

                case UnaryOperationType.Minus:
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to apply unary - on {operand.Type}.",
                            Environment,
                            unaryOperationNode.Line
                        );
                    }
                    
                    return new DynValue(-operand.Number);

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
                    if (unaryOperationNode.Operand is VariableNode variable)
                        return new DynValue(variable.Identifier);

                    throw new RuntimeException(
                        "Attempt to get a name of a non-variable symbol.",
                        Environment,
                        unaryOperationNode.Line
                    );

                case UnaryOperationType.Negation:
                    if (operand.IsTruth)
                        return new DynValue(0);
                    else
                        return new DynValue(1);

                case UnaryOperationType.BitwiseNot:
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to negate a {operand.Type}.",
                            Environment,
                            unaryOperationNode.Line
                        );
                    }

                    return new DynValue(~(long)operand.Number);

                case UnaryOperationType.Floor:
                    if (operand.Type != DynValueType.Number)
                    {
                        throw new RuntimeException(
                            $"Attempt to retrieve floor value of {operand.Type}.",
                            Environment,
                            unaryOperationNode.Line
                        );
                    }
                    
                    return new DynValue(decimal.Floor(operand.Number));

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