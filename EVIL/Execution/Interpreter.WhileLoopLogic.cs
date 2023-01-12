using EVIL.Abstraction;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
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

                    while (Visit(whileLoopNode.Expression).IsTruth)
                    {
                        Environment.EnterScope();
                        {
                            ExecuteStatementList(whileLoopNode.StatementList);
                        }
                        Environment.ExitScope();
                        var stackTop = Environment.LoopStackTop;

                        if (stackTop.BreakLoop)
                            break;

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
