using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableNode variableNode)
        {
            if (CallStack.Count > 0)
            {
                var stackTop = CallStack.Peek();

                if (stackTop.ParameterScope.ContainsKey(variableNode.Name))
                    return stackTop.ParameterScope[variableNode.Name];

                if (stackTop.LocalVariableScope.ContainsKey(variableNode.Name))
                    return stackTop.LocalVariableScope[variableNode.Name];

                if (Environment.SupplementLocalLookupTable.ContainsKey(variableNode.Name))
                    return Environment.SupplementLocalLookupTable[variableNode.Name];
            }

            if (!Environment.Globals.ContainsKey(variableNode.Name))
                throw new RuntimeException($"The referenced variable '{variableNode.Name}' was never defined.", variableNode.Line);

            return Environment.Globals[variableNode.Name];
        }
    }
}
