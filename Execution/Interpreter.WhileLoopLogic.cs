using EVIL.Abstraction;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(WhileLoopNode whileLoopNode)
        {
            try
            {
                LoopStack.Push(new LoopStackItem());

                while (Visit(whileLoopNode.Expression).Number != 0)
                {
                    ExecuteStatementList(whileLoopNode.StatementList);

                    var stackTop = LoopStack.Peek();

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
                LoopStack.Pop();
            }

            return DynValue.Zero;
        }

    }
}
