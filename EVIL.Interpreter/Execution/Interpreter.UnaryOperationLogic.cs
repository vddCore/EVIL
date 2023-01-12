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
            
            DynValue metaResult = null;
            switch (unaryOperationNode.Type)
            {
                case UnaryOperationType.Plus:
                    if (ExecuteUnaryMeta("__unp", operand, unaryOperationNode, out metaResult))
                        return metaResult;
                    
                    if (operand.Type != DynValueType.Number)
                        throw new RuntimeException($"Attempt to apply unary + on {operand.Type}.",
                            unaryOperationNode.Line);

                    return new DynValue(operand.Number);

                case UnaryOperationType.Minus:
                    if (ExecuteUnaryMeta("__unm", operand, unaryOperationNode, out metaResult))
                        return metaResult;
                    
                    if (operand.Type != DynValueType.Number)
                        throw new RuntimeException($"Attempt to apply unary - on {operand.Type}.",
                            unaryOperationNode.Line);

                    return new DynValue(-operand.Number);

                case UnaryOperationType.Length:
                {
                    if (ExecuteUnaryMeta("__len", operand, unaryOperationNode, out metaResult))
                        return metaResult;
                    
                    switch (operand.Type)
                    {
                        case DynValueType.String:
                            return new DynValue(operand.String.Length);
                        case DynValueType.Table:
                            return new DynValue(operand.Table.Count);
                        default:
                            throw new RuntimeException($"Attempt to retrieve the length of {operand.Type}.",
                                unaryOperationNode.Line);
                    }
                }

                case UnaryOperationType.ToString:
                    if (ExecuteUnaryMeta("__str", operand, unaryOperationNode, out metaResult))
                        return metaResult;
                    
                    return operand.AsString();

                case UnaryOperationType.NameOf:
                    if (ExecuteUnaryMeta("__nameof", operand, unaryOperationNode, out metaResult))
                        return metaResult;

                    if (unaryOperationNode.Operand is VariableNode variable)
                        return new DynValue(variable.Identifier);

                    throw new RuntimeException("Attempt to get a name of a non-variable symbol.",
                        unaryOperationNode.Line);

                case UnaryOperationType.Negation:
                    if (ExecuteUnaryMeta("__not", operand, unaryOperationNode, out metaResult))
                        return metaResult;

                    if (operand.IsTruth)
                        return new DynValue(0);
                    else
                        return new DynValue(1);

                case UnaryOperationType.BitwiseNot:
                    if (ExecuteUnaryMeta("__bnot", operand, unaryOperationNode, out metaResult))
                        return metaResult;
                    
                    if (operand.Type != DynValueType.Number)
                        throw new RuntimeException($"Attempt to negate a {operand.Type}.", unaryOperationNode.Line);

                    return new DynValue(~(long)operand.Number);

                case UnaryOperationType.Floor:
                    if (ExecuteUnaryMeta("__floor", operand, unaryOperationNode, out metaResult))
                        return metaResult;
                    
                    if (operand.Type != DynValueType.Number)
                        throw new RuntimeException($"Attempt to retrieve floor value of {operand.Type}.",
                            unaryOperationNode.Line);

                    return new DynValue(decimal.Floor(operand.Number));

                default: throw new RuntimeException("Unknown unary operation type.", unaryOperationNode.Line);
            }
        }
        
        private bool ExecuteUnaryMeta(string identifier, DynValue operand, AstNode node, out DynValue value)
        {
            var meta = operand.Meta[identifier];

            if (meta.IsTruth)
            {
                if (meta.Type == DynValueType.Function)
                {
                    var args = new FunctionArguments();
                    args.Add(operand);

                    Environment.EnterScope(true);
                    {
                        value = ExecuteScriptFunction(meta.ScriptFunction, identifier, args, node);
                    }
                    Environment.ExitScope();
                }
                else value = meta;
                return true;
            }
            else
            {
                value = DynValue.Zero;
                return false;
            }
        }
    }
}