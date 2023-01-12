using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(ForStatement forStatement)
        {
            Environment.EnterScope();
            {
                for (var i = 0; i < forStatement.Assignments.Count; i++)
                {
                    Visit(forStatement.Assignments[i]);
                }

                try
                {
                    var loopStackTop = new LoopFrame();
                    CallStack.Peek().LoopStack.Push(loopStackTop);

                    while (true)
                    {
                        var conditionEvaluation = Visit(forStatement.Condition);
                        if (!conditionEvaluation.IsTruth)
                        {
                            break;
                        }

                        if (!loopStackTop.SkipThisIteration)
                        {
                            Visit(forStatement.Statement);
                        }

                        if (loopStackTop.BreakLoop)
                        {
                            break;
                        }

                        if (loopStackTop.SkipThisIteration)
                        {
                            loopStackTop.SkipThisIteration = false;
                        }

                        for (var i = 0; i < forStatement.IterationExpressions.Count; i++)
                        {
                            Visit(forStatement.IterationExpressions[i]);
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