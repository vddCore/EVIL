using System.Linq;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(BlockStatement blockStatement)
        {
            if (blockStatement.Statements.Count > 0)
            {
                Environment.EnterScope();
                {
                    for (var i = 0; i < blockStatement.Statements.Count; i++)
                    {
                        var callStackTop = StackTop;

                        if (callStackTop.ReturnNow)
                        {
                            for (var j = 0; j < callStackTop.LoopStack.Count; j++)
                                callStackTop.LoopStack.ElementAt(j).Break();

                            break;
                        }

                        if (callStackTop.IsInLoop)
                        {
                            var loopStackTop = callStackTop.LoopStackTop;

                            if (loopStackTop.BreakLoop || loopStackTop.SkipThisIteration)
                                break;
                        }

                        Visit(blockStatement.Statements[i]);
                    }
                }
                Environment.ExitScope();
            }
        }
    }
}