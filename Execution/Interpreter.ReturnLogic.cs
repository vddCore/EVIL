using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ReturnNode returnNode)
        {
            if (CallStack.Count <= 0)
                throw new RuntimeException("Return statement outside of a function.", returnNode.Line);

            var stackTop = CallStack.Peek();
            stackTop.ReturnNow = true;
            stackTop.ReturnValue = Visit(returnNode.Expression);

            return stackTop.ReturnValue;
        }
    }
}
