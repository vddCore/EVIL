using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableNode variableNode)
        {
            var dynValue = Environment.LocalScope.FindInScopeChain(variableNode.Identifier);

            if (dynValue == null)
            {
                throw new RuntimeException(
                    $"'{variableNode.Identifier}' does not exist in the current scope.",
                    Environment,
                    variableNode.Line
                );
            }

            return dynValue;
        }
    }
}