using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(IndexingNode indexingNode)
        {
            var variable = Visit(indexingNode.Variable);
            var keyValue = Visit(indexingNode.KeyExpression);

            switch (variable.Type)
            {
                case DynValueType.String:
                {
                    if (keyValue.Type != DynValueType.Number)
                        throw new RuntimeException("Strings can only be indexed using numbers.", indexingNode.Line);

                    var index = (int)keyValue.Number;

                    if (index < 0)
                    {
                        if (variable.String.Length + index < 0)
                        {
                            throw new RuntimeException("String index out of bounds.", indexingNode.Line);
                        }

                        return new DynValue(
                            variable.String[^(-index)]
                        );
                    }
                    else
                    {
                        if (index >= variable.String.Length)
                        {
                            throw new RuntimeException("String index out of bounds.", indexingNode.Line);
                        }
                        
                        return new DynValue(
                            variable.String[index].ToString()
                        );
                    }
                }

                case DynValueType.Number:
                {
                    throw new RuntimeException($"Numbers cannot be indexed.", indexingNode.Line);
                }

                case DynValueType.Table:
                {
                    if (keyValue.Type == DynValueType.String)
                        return variable.Table[keyValue.String];
                    else if (keyValue.Type == DynValueType.Number)
                        return variable.Table[keyValue.Number];
                    else
                        throw new RuntimeException($"Type '{keyValue.Type}' cannot be used as a key.",
                            indexingNode.Line);
                }

                default:
                {
                    throw new RuntimeException("This type cannot be indexed.", indexingNode.Line);
                }
            }
        }
    }
}