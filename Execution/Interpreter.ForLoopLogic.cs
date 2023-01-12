using EVIL.Abstraction;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ForLoopNode forLoopNode)
        {
            _ = Visit(forLoopNode.Assignment).Number;

            DynValue step = new DynValue(1);

            var targetValue = Visit(forLoopNode.TargetValue);

            if (forLoopNode.Step != null)
                step = Visit(forLoopNode.Step);

            var isLocalScope = false;

            var assignmentNode = (forLoopNode.Assignment as AssignmentNode);
            var iteratorName = assignmentNode.Variable.Name;

            if (assignmentNode.LocalScope)
                isLocalScope = true;

            try
            {
                LoopStack.Push(new LoopStackItem());
                while (true)
                {
                    ExecuteStatementList(forLoopNode.StatementList);

                    double iterator;
                    if (isLocalScope)
                    {
                        var callStackTop = CallStack.Peek();
                        iterator = callStackTop.LocalVariableScope[iteratorName].Number;
                    }
                    else
                    {
                        iterator = Environment.Globals[iteratorName].Number;
                    }

                    if (step.Number < 0)
                    {
                        if (iterator < targetValue.Number)
                            break;
                        else iterator -= step.Number;
                    }
                    else
                    {
                        if (iterator >= targetValue.Number)
                            break;
                        else iterator += step.Number;
                    }

                    if (isLocalScope)
                    {
                        // no throw on localvar token, so
                        // guaranteed we're inside a function

                        var callStackTop = CallStack.Peek();
                        callStackTop.LocalVariableScope[iteratorName] = new DynValue(iterator);
                    }
                    else Environment.Globals[iteratorName] = new DynValue(iterator);

                    var loopStackTop = LoopStack.Peek();

                    if (loopStackTop.BreakLoop)
                        break;

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
