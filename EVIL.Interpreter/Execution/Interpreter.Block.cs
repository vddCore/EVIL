using EVIL.Grammar.AST;
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
                                break;
                            }
                        }
                        
                        Visit(blockStatement.Statements[i]);
                    }
                }
                Environment.ExitScope();
            }
        }
    }
}