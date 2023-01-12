using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(NameOfExpression nameOfExpression)
        {
            if (nameOfExpression.Right is VariableReferenceExpression variable)
            {
                var dynValue = Environment.LocalScope?.FindInScope(variable.Identifier)
                               ?? Environment.GlobalScope.FindInScope(variable.Identifier);

                if (dynValue != null)
                    return new DynValue(variable.Identifier);

                return DynValue.Zero;
            }
            else if (nameOfExpression.Right is IndexerExpression indexingNode)
            {
                try
                {
                    Visit(indexingNode);
                }
                catch
                {
                    return DynValue.Zero;
                }

                return new DynValue(indexingNode.BuildChainStringRepresentation());
            }

            throw new RuntimeException(
                "Attempt to get a name of a non-variable symbol.",
                Environment,
                nameOfExpression.Line
            );
        }
    }
}