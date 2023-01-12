using System.Collections.Generic;
using System.Linq;
using EVIL.Abstraction;
using EVIL.AST.Base;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(IndexingNode indexingNode)
        {
            var q = new Queue<AstNode>(indexingNode.KeyExpressions);

            var indexable = Visit(indexingNode.Indexable);
            DynValue keyValue;

            do
            {
                keyValue = Visit(q.Dequeue());
                indexable = IndexDynValue(indexable, keyValue, indexingNode);
            } while (q.Any());

            return indexable;
        }

        private DynValue IndexDynValue(DynValue indexable, DynValue keyValue, IndexingNode indexingNode)
        {
            switch (indexable.Type)
            {
                case DynValueType.String:
                {
                    if (keyValue.Type != DynValueType.Number)
                        throw new RuntimeException($"Attempt to index a string using {keyValue.Type}.",
                            indexingNode.Line);

                    var index = (int)keyValue.Number;

                    if (index < 0)
                    {
                        if (indexable.String.Length + index < 0)
                        {
                            throw new RuntimeException("String index out of bounds.", indexingNode.Line);
                        }

                        return new DynValue(
                            indexable.String[^(-index)]
                        );
                    }
                    else
                    {
                        if (index >= indexable.String.Length)
                        {
                            throw new RuntimeException("String index out of bounds.", indexingNode.Line);
                        }

                        return new DynValue(
                            indexable.String[index].ToString()
                        );
                    }
                }

                case DynValueType.Number:
                {
                    throw new RuntimeException($"Attempt to index a {DynValueType.Number}", indexingNode.Line);
                }

                case DynValueType.Table:
                {
                    try
                    {
                        if (keyValue.Type == DynValueType.String)
                            return indexable.Table[keyValue.String];
                        else if (keyValue.Type == DynValueType.Number)
                            return indexable.Table[keyValue.Number];
                        else
                            throw new RuntimeException($"Attempt to use {keyValue.Type} as a key.", indexingNode.Line);
                    }
                    catch
                    {
                        if (!indexingNode.WillBeAssigned) throw;
                        indexable.Table[keyValue] = new DynValue(0);

                        return indexable.Table[keyValue];
                    }
                }

                default:
                {
                    throw new RuntimeException($"Attempt to index {indexable.Type}.", indexingNode.Line);
                }
            }
        }
    }
}