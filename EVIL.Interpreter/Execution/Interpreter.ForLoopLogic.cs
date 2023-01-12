using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ForLoopNode forLoopNode)
        {
            Environment.EnterScope();
            {
                foreach (var assignment in forLoopNode.Assignments)
                {
                    Visit(assignment);
                }

                try
                {
                    Environment.LoopStack.Push(new LoopFrame());
                    while (true)
                    {
                        var conditionEvaluation = Visit(forLoopNode.Condition);
                        if (!conditionEvaluation.IsTruth)
                        {
                            break;
                        }

                        var loopStackTop = Environment.LoopStack.Peek();

                        if (loopStackTop.BreakLoop)
                        {
                            break;
                        }

                        if (!loopStackTop.SkipThisIteration)
                        {
                            Environment.EnterScope();
                            {
                                ExecuteStatementList(forLoopNode.StatementList);
                            }
                            Environment.ExitScope();
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
                    Environment.LoopStack.Pop();
                }
            }
            Environment.ExitScope();

            return DynValue.Zero;
        }
    }
}