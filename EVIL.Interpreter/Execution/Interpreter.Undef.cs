using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(UndefStatement undefStatement)
        {
            if (undefStatement.Right is IndexerExpression indexingNode)
            {
                var indexable = Visit(indexingNode.Indexable);
                var key = Visit(indexingNode.KeyExpression);

                if (indexable.Type != DynValueType.Table)
                {
                    throw new RuntimeException(
                        "Attempt to undefine a member of a type which is not a Table.",
                        Environment,
                        undefStatement.Line
                    );
                }

                indexable.Table.Remove(key);
            }
            else if (undefStatement.Right is VariableReference variable)
            {
                Environment.LocalScope.UnSetInChain(variable.Identifier);
            }
            else
            {
                throw new RuntimeException(
                    "Expected a variable or an indexer.",
                    Environment,
                    undefStatement.Line
                );
            }
        }
    }
}