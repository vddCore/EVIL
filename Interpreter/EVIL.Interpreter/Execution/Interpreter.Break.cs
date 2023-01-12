using EVIL.Grammar.AST.Nodes;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(BreakStatement breakStatement)
        {
            Environment.CallStack.Peek()
                .LoopStackTop.Break();
        }
    }
}