using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ReturnNode returnNode)
        {
            if (!Environment.IsInScriptFunctionScope)
                throw new RuntimeException("Return statement outside of a function.", returnNode.Line);

            var stackTop = Environment.StackTop;
            stackTop.ReturnValue = Visit(returnNode.Expression);
            stackTop.Return();

            return stackTop.ReturnValue;
        }
    }
}
