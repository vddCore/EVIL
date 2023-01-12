using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableNode variableNode)
        {
            var dynValue = Environment.LocalScope.FindInScopeChain(variableNode.Identifier);

            if (dynValue == null)
            {
                throw new RuntimeException($"'{variableNode.Identifier}' does not exist in the current scope.", variableNode.Line);
            }

            return dynValue;
        }
    }
}
