using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ExitNode exitNode)
        {
            throw new ExitStatementException();
        }
    }
}
