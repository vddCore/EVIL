using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UndefNode undefNode)
        {
            Environment.LocalScope.UnSetInChain(undefNode.Identifier);
            return DynValue.Zero;
        }
    }
}
