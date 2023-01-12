using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BreakNode breakNode)
        {
            if (LoopStack.Count <= 0)
                throw new RuntimeException("Unexpected 'break' outside a loop.", breakNode.Line);

            LoopStack.Peek().BreakLoop = true;

            return DynValue.Zero;
        }

    }
}
