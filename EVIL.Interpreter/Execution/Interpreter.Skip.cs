using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(SkipNode skipNode)
        {
            Environment.LoopStackTop.SkipThisIteration = true;
            return DynValue.Zero;
        }
    }
}