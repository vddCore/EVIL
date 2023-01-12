using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(NumberNode numberNode)
        {
            return new(numberNode.Value);
        }

        public override DynValue Visit(StringNode stringNode)
        {
            return new(stringNode.Value);
        }

        public override DynValue Visit(TableNode tableNode)
        {
            var tbl = new Table();

            for (var i = 0; i < tableNode.Initializers.Count; i++)
            {
                tbl[i] = Visit(tableNode.Initializers[i]);
            }

            return new(tbl);
        }
    }
}