using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UnaryOperationNode unaryOperationNode)
        {
            var operand = Visit(unaryOperationNode.Operand);

            switch (unaryOperationNode.Type)
            {
                case UnaryOperationType.Plus:
                    return new DynValue(+operand.Number);

                case UnaryOperationType.Minus:
                    return new DynValue(-operand.Number);

                case UnaryOperationType.Length:
                {
                    switch (operand.Type)
                    {
                        case DynValueType.String:
                            return new DynValue(operand.String.Length);
                        case DynValueType.Table:
                            return new DynValue(operand.Table.Count);
                        default: throw new RuntimeException($"Length operator doesn't support the type '{operand.Type}'", unaryOperationNode.Line);
                    }
                }

                case UnaryOperationType.ToString:
                    return operand.AsString();

                case UnaryOperationType.NameOf:
                {
                    if (unaryOperationNode.Operand is VariableNode variable)
                        return new DynValue(variable.Name);
                    
                    throw new RuntimeException("Attempt to get a name of a non-variable symbol.", unaryOperationNode.Line);
                }

                case UnaryOperationType.Negation:
                {
                    if (operand.Type != DynValueType.Number)
                        throw new RuntimeException($"Attempt of logical negation of non-numerical type '{operand.Type}'", unaryOperationNode.Line);

                    if (operand.Number != 0)
                        return new DynValue(0);
                    else
                        return new DynValue(1);
                }
                
                case UnaryOperationType.BitwiseNot:
                    return new DynValue(~(int)operand.Number);

                default: throw new RuntimeException("Unknown unary operation type.", unaryOperationNode.Line);
            }
        }
    }
}
