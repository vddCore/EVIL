using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(DoWhileLoopNode doWhileLoopNode)
        {
            Visit(doWhileLoopNode.Statement);

            while (Visit(doWhileLoopNode.ConditionExpression).IsTruth)
            {
                Environment.LoopStack.Push(new LoopFrame());
                var loopStackTop = Environment.LoopStack.Peek();

                try
                {
                    if (!loopStackTop.SkipThisIteration)
                    {
                        Visit(doWhileLoopNode.Statement);
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

            return DynValue.Zero;
        }
    }
}