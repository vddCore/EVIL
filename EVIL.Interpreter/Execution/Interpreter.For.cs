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
                for (var i = 0; i < forLoopNode.Assignments.Count; i++)
                {
                    Visit(forLoopNode.Assignments[i]);
                }

                try
                {
                    Environment.LoopStack.Push(new LoopFrame());
                    var loopStackTop = Environment.LoopStack.Peek();

                    while (true)
                    {
                        var conditionEvaluation = Visit(forLoopNode.Condition);
                        if (!conditionEvaluation.IsTruth)
                        {
                            break;
                        }

                        if (!loopStackTop.SkipThisIteration)
                        {
                            Visit(forLoopNode.Statement);
                        }

                        if (loopStackTop.BreakLoop)
                        {
                            break;
                        }

                        if (loopStackTop.SkipThisIteration)
                        {
                            loopStackTop.SkipThisIteration = false;
                        }

                        for (var i = 0; i < forLoopNode.IterationStatements.Count; i++)
                        {
                            Visit(forLoopNode.IterationStatements[i]);
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