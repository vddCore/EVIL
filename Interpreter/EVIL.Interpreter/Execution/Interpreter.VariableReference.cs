using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableReferenceExpression variableReferenceExpression)
        {
            var dynValue = Environment.LocalScope?.FindInScope(variableReferenceExpression.Identifier)
                           ?? Environment.GlobalScope.FindInScope(variableReferenceExpression.Identifier);

            if (dynValue == null)
            {
                throw new RuntimeException(
                    $"'{variableReferenceExpression.Identifier}' does not exist in the current scope.",
                    Environment,
                    variableReferenceExpression.Line
                );
            }

            return dynValue;
        }
    }
}