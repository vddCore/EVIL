using EVIL.Abstraction;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ForLoopNode forLoopNode)
        {
            foreach (var assignment in forLoopNode.Assignments)
            {
                Visit(assignment);
            }

            try
            {
                LoopStack.Push(new LoopStackItem());
                while (true)
                {
                    var conditionEvaluation = Visit(forLoopNode.Condition);
                    if (conditionEvaluation.Number == 0)
                    {
                        break;
                    }
                    
                    var loopStackTop = LoopStack.Peek();

                    if (loopStackTop.BreakLoop)
                    {
                        break;
                    }
                    
                    if (!loopStackTop.SkipThisIteration)
                    {
                        ExecuteStatementList(forLoopNode.StatementList);
                    }

                    foreach (var iterationStatement in forLoopNode.IterationStatements)
                    {
                        Visit(iterationStatement);
                    }

                    if (loopStackTop.SkipThisIteration)
                    {
                        loopStackTop.SkipThisIteration = false;
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
