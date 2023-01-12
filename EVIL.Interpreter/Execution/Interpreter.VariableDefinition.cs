using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableDefinitionNode variableDefinitionNode)
        {
            var identifier = variableDefinitionNode.Identifier;

            if (Environment.LocalScope.HasMember(identifier))
            {
                throw new RuntimeException(
                    $"Variable '{identifier}' was already defined in the current scope.", 
                    Environment,
                    variableDefinitionNode.Line
                );
            }

            var dynValue = Environment.LocalScope.Set(identifier, DynValue.Zero);
            dynValue.CopyFrom(Visit(variableDefinitionNode.Right));

            return dynValue;
        }
    }
}