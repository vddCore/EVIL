using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ParameterListNode parameterListNode)
        {
            if (_preCallStack.Count == 0)
            {
                throw new RuntimeException(
                    "Should never happen. Parameter list visited without a function call.",
                    Environment,
                    parameterListNode.Line
                );
            }

            var preCallStackFrame = _preCallStack.Peek();
            for (var i = 0; i < parameterListNode.Identifiers.Count; i++)
            {
                preCallStackFrame.Parameters.Add(parameterListNode.Identifiers[i]);
            }
            
            return DynValue.Zero;
        }
    }
}