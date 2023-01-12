using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(SkipNode skipNode)
        {
            if (!Environment.IsInsideLoop)
            {
                throw new RuntimeException(
                    "Unexpected 'skip' outside of a loop.",
                    Environment,
                    skipNode.Line);
            }

            Environment.LoopStackTop.SkipThisIteration = true;
            return DynValue.Zero;
        }
    }
}
