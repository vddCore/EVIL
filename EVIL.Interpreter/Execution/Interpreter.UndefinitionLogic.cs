using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UndefNode undefNode)
        {
            if (undefNode.Symbol is IndexingNode indexingNode)
            {
                var indexable = Visit(indexingNode.Indexable);
                var key = Visit(indexingNode.KeyExpression);

                if (indexable.Type != DynValueType.Table)
                {
                    throw new RuntimeException(
                        "Attempt to undefine a member of a type which is not a Table.",
                        Environment,
                        undefNode.Line
                    );
                }
                
                indexable.Table.Remove(indexable.Table.GetKeyByDynValue(key));
            }
            else if (undefNode.Symbol is VariableNode variable)
            {
                Environment.LocalScope.UnSetInChain(variable.Identifier);
            }
            else
            {
                throw new RuntimeException(
                    "Expected a variable or an indexer.",
                    Environment,
                    undefNode.Line
                );
            }

            return DynValue.Zero;
        }
    }
}