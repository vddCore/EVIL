using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(NumberNode numberNode)
        {
            return numberNode.Value;
        }

        public override DynValue Visit(StringNode stringNode)
        {
            return stringNode.Value;
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