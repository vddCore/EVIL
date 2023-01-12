using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableDefinitionNode variableDefinitionNode)
        {
            var identifier = variableDefinitionNode.Variable.Identifier;

            if (Environment.LocalScope.HasMember(identifier))
            {
                throw new RuntimeException(
                    $"Variable '{identifier}' was already defined in the current scope.", variableDefinitionNode.Line
                );
            }

            var dynValue = Environment.LocalScope.Set(identifier, new DynValue(0));
            dynValue.CopyFrom(Visit(variableDefinitionNode.Right));

            return dynValue;
        }
    }
}