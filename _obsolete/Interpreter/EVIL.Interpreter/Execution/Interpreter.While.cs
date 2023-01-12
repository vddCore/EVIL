using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(WhileStatement whileStatement)
        {
            Environment.EnterScope();
            {
                try
                {
                    var stackTop = new LoopFrame();
                    CallStack.Peek().LoopStack.Push(stackTop);

                    while (Visit(whileStatement.Expression).IsTruth)
                    {
                        Visit(whileStatement.Statement);

                        if (stackTop.BreakLoop)
                        {
                            break;
                        }

                        if (stackTop.SkipThisIteration)
                        {
                            stackTop.SkipThisIteration = false;
                        }
                    }
                }
                finally
                {
                    CallStack.Peek().LoopStack.Pop();
                }
            }
            Environment.ExitScope();
        }
    }
}