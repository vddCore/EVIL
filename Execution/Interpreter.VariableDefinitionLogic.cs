using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableDefinitionNode variableDefinitionNode)
        {
            var identifier = variableDefinitionNode.Variable.Identifier;
            
            if (Environment.LocalScope.FindInScopeChain(identifier) != null)
            {
                throw new RuntimeException(
                    $"Variable '{identifier}' was already defined in this scope.", variableDefinitionNode.Line
                );
            }

            var dynValue = Environment.LocalScope.Set(identifier, new DynValue(0));
            dynValue.CopyFrom(Visit(variableDefinitionNode.Assignment));

            return dynValue;
        }
    }
}