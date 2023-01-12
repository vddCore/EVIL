using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(ReturnStatement returnStatement)
        {
            var stackTop = StackTop;

            if (returnStatement.Expression != null)
            {
                stackTop.ReturnValue = Visit(returnStatement.Expression);
            }

            stackTop.Return();
        }
    }
}