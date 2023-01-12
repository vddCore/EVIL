using System;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(IndexerExpression indexerExpression)
        {
            var indexable = Visit(indexerExpression.Indexable);
            var keyValue = Visit(indexerExpression.KeyExpression);
            var retValue = IndexDynValue(indexable, keyValue, indexerExpression);

            return retValue;
        }

        private DynValue IndexDynValue(DynValue indexable, DynValue keyValue, IndexerExpression indexerExpression)
        {
            if (indexable.Type == DynValueType.String)
            {
                if (keyValue.Type != DynValueType.Number)
                {
                    throw new RuntimeException(
                        $"Attempt to index a string using {keyValue.Type}.",
                        Environment,
                        indexerExpression.Line
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
                            indexerExpression.Line
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
                            indexerExpression.Line
                        );
                    }

                    return new DynValue(
                        indexable.String[index].ToString()
                    );
                }
            }
            else if (indexable.Type == DynValueType.Table)
            {
                if (keyValue.Type == DynValueType.String)
                {
                    if (indexerExpression.WillBeAssigned)
                    {
                        indexable.Table[keyValue] = DynValue.Zero;
                        return indexable.Table[keyValue];
                    }

                    return indexable.Table[keyValue.String]
                           ?? throw new RuntimeException(
                               $"'{keyValue.String}' does not exist in the table.", Environment, indexerExpression.Line
                           );
                }
                else if (keyValue.Type == DynValueType.Number)
                {
                    if (indexerExpression.WillBeAssigned)
                    {
                        indexable.Table[keyValue] = DynValue.Zero;
                        return indexable.Table[keyValue];
                    }

                    return indexable.Table[keyValue.Number]
                           ?? throw new RuntimeException(
                               $"'{keyValue.String}' does not exist in the table.", Environment, indexerExpression.Line
                           );
                }
                else
                {
                    throw new RuntimeException(
                        $"Attempt to use {keyValue.Type} as a key.",
                        Environment,
                        indexerExpression.Line
                    );
                }
            }

            throw new RuntimeException(
                $"Attempt to index a {indexable.Type}.",
                Environment,
                indexerExpression.Line
            );
        }
    }
}