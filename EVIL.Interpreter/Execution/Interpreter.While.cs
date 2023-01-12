using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(WhileLoopNode whileLoopNode)
        {
            Environment.EnterScope();
            {
                try
                {
                    Environment.LoopStack.Push(new LoopFrame());
                    var stackTop = Environment.LoopStackTop;

                    while (Visit(whileLoopNode.Expression).IsTruth)
                    {
                        Visit(whileLoopNode.Statements);

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

            return DynValue.Zero;
        }
    }
}