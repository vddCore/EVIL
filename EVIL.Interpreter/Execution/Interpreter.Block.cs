using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BlockStatementNode blockStatementNode)
        {
            var retVal = DynValue.Zero;

            if (blockStatementNode.Statements.Count > 0)
            {
                Environment.EnterScope();
                {
                    for (var i = 0; i < blockStatementNode.Statements.Count; i++)
                    {
                        if (Environment.IsInsideLoop)
                        {
                            var loopStackTop = Environment.LoopStackTop;

                            if (loopStackTop.BreakLoop || loopStackTop.SkipThisIteration)
                                break;
                        }

                        if (Environment.IsInScriptFunctionScope)
                        {
                            var callStackTop = Environment.StackTop;

                            if (callStackTop.ReturnNow)
                            {
                                retVal = callStackTop.ReturnValue;
                                break;
                            }
                        }

                        retVal = Visit(blockStatementNode.Statements[i]);
                    }
                }
                Environment.ExitScope();
            }

            return retVal;
        }
    }
}