using System;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(IndexingNode indexingNode)
        {
            var indexable = Visit(indexingNode.Indexable);
            var keyValue = Visit(indexingNode.KeyExpression);
            var retValue = IndexDynValue(indexable, keyValue, indexingNode);

            return retValue;
        }

        private DynValue IndexDynValue(DynValue indexable, DynValue keyValue, IndexingNode indexingNode)
        {
            if (indexable.Type == DynValueType.String)
            {
                if (keyValue.Type != DynValueType.Number)
                {
                    throw new RuntimeException(
                        $"Attempt to index a string using {keyValue.Type}.",
                        Environment,
                        indexingNode.Line
                    );
                }

                var index = (int)keyValue.Number;

                if (index < 0)
                {
                    if (indexable.String.Length + index < 0)
                    {
                        throw new RuntimeException(
                            "String index out of bounds.",
                            Environment,
                            indexingNode.Line
                        );
                    }

                    return new DynValue(
                        indexable.String[^(-index)]
                    );
                }
                else
                {
                    if (index >= indexable.String.Length)
                    {
                        throw new RuntimeException(
                            "String index out of bounds.",
                            Environment,
                            indexingNode.Line
                        );
                    }

                    return new DynValue(
                        indexable.String[index].ToString()
                    );
                }
            }
            else if (indexable.Type == DynValueType.Table)
            {
                try
                {
                    if (keyValue.Type == DynValueType.String)
                        return indexable.Table[keyValue.String];
                    else if (keyValue.Type == DynValueType.Number)
                        return indexable.Table[keyValue.Number];
                    else
                    {
                        throw new RuntimeException(
                            $"Attempt to use {keyValue.Type} as a key.",
                            Environment,
                            indexingNode.Line
                        );
                    }
                }
                catch (Exception e)
                {
                    if (!indexingNode.WillBeAssigned)
                    {
                        throw new RuntimeException(
                            e.Message,
                            Environment,
                            indexingNode.Line
                        );
                    }

                    indexable.Table[keyValue] = new DynValue(0);
                    return indexable.Table[keyValue];
                }
            }

            throw new RuntimeException(
                $"Attempt to index a {indexable.Type}.",
                Environment,
                indexingNode.Line
            );
        }
    }
}