using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(TableExpression tableExpression)
        {
            var tbl = new Table();

            if (tableExpression.Keyed)
            {
                for (var i = 0; i < tableExpression.Initializers.Count; i++)
                {
                    var node = (KeyValuePairExpression)tableExpression.Initializers[i];
                    var key = Visit(node.KeyNode);

                    if (tbl.ContainsKey(key))
                    {
                        throw new RuntimeException(
                            $"Attempt to initialize a table with the same key '{key.AsString().String}' twice.",
                            this,
                            node.Line
                        );
                    }

                    tbl[key] = Visit(node.ValueNode);
                }
            }
            else
            {
                for (var i = 0; i < tableExpression.Initializers.Count; i++)
                {
                    tbl[i] = Visit(tableExpression.Initializers[i]);
                }
            }

            return new DynValue(tbl);
        }
    }
}