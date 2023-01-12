using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(DoWhileStatement doWhileStatement)
        {
            Visit(doWhileStatement.Statement);

            while (Visit(doWhileStatement.Condition).IsTruth)
            {
                var loopStackTop = new LoopFrame();
                CallStack.Peek().LoopStack.Push(loopStackTop);

                try
                {
                    if (!loopStackTop.SkipThisIteration)
                    {
                        Visit(doWhileStatement.Statement);
                    }

                    if (loopStackTop.BreakLoop)
                    {
                        break;
                    }

                    if (loopStackTop.SkipThisIteration)
                    {
                        loopStackTop.SkipThisIteration = false;
                    }
                }
                finally
                {
                    CallStack.Peek().LoopStack.Pop();
                }
            }
        }
    }
}