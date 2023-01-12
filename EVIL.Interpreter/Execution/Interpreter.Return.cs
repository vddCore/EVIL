using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ReturnNode returnNode)
        {
            var stackTop = Environment.StackTop;

            if (returnNode.Right != null)
            {
                stackTop.ReturnValue = Visit(returnNode.Right);
            }
            
            stackTop.Return();
            return stackTop.ReturnValue;
        }
    }
}
