using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
            var stackFrame = Environment.CallStack.Peek();
            var tbl = new Table();
            var dv = new DynValue(tbl);

            var j = 0;
            for (var i = stackFrame.Parameters.Count; i < stackFrame.Arguments.Count; i++, j++)
            {
                tbl[j] = stackFrame.Arguments[i];
            }

            return dv;
        }
    }
}