using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(BreakStatement breakStatement)
        {
            Environment.LoopStackTop.Break();
        }
    }
}