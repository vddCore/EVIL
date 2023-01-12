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
                Environment.LoopStack.Push(new LoopFrame());
                var loopStackTop = Environment.LoopStack.Peek();

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
                    Environment.LoopStack.Pop();
                }
            }
        }
    }
}