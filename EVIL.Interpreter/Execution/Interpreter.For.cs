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
                            Visit(forLoopNode.Statements);
                        }
                        
                        if (loopStackTop.BreakLoop)
                        {
                            break;
                        }

                        if (loopStackTop.SkipThisIteration)
                        {
                            loopStackTop.SkipThisIteration = false;
                        }

                        foreach (var iterationStatement in forLoopNode.IterationStatements)
                        {
                            Visit(iterationStatement);
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