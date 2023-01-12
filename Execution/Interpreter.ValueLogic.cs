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
            return new(new Table());
        }
    }
}
