using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(SkipNode skipNode)
        {
            if (!Environment.IsInsideLoop)
                throw new RuntimeException("Unexpected 'skip' outside of a loop.", skipNode.Line);

            Environment.LoopStackTop.SkipThisIteration = true;
            return DynValue.Zero;
        }
    }
}
