using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BreakNode breakNode)
        {
            if (!Environment.IsInsideLoop)
                throw new RuntimeException("Unexpected 'break' outside a loop.", breakNode.Line);

            Environment.LoopStackTop.Break();
            return DynValue.Zero;
        }
    }
}
