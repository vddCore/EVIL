using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableNode variableNode)
        {
            if (Environment.IsInScriptFunctionScope)
            {
                var stackTop = Environment.CallStackTop;

                if (stackTop.HasParameter(variableNode.Name))
                    return stackTop.Parameters[variableNode.Name];
            }

            return Environment.LocalScope.FindInScopeChain(variableNode.Name);
        }
    }
}
