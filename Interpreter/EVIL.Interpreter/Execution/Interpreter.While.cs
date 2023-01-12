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
                    Environment.LoopStack.Push(new LoopFrame());
                    var stackTop = Environment.LoopStackTop;

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
                    Environment.LoopStack.Pop();
                }
            }
            Environment.ExitScope();
        }
    }
}