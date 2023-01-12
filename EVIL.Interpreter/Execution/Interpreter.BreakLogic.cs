using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BreakNode breakNode)
        {
            if (!Environment.IsInsideLoop)
            {
                throw new RuntimeException(
                    "Unexpected 'break' outside a loop.", 
                    Environment,
                    breakNode.Line
                );
            }
            Environment.LoopStackTop.Break();
            return DynValue.Zero;
        }
    }
}
