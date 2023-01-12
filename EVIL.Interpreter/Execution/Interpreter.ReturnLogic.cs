using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
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
