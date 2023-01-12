using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ReturnNode returnNode)
        {
            if (!Environment.IsInScriptFunctionScope)
            {
                throw new RuntimeException(
                    "Return statement outside of a function.",
                    Environment, returnNode.Line
                );
            }

            var stackTop = Environment.StackTop;
            stackTop.ReturnValue = Visit(returnNode.Right);
            stackTop.Return();

            return stackTop.ReturnValue;
        }
    }
}
