using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(VariableDefinition variableDefinition)
        {
            var identifier = variableDefinition.Identifier;

            if (Environment.LocalScope.HasMember(identifier))
            {
                throw new RuntimeException(
                    $"Variable '{identifier}' was already defined in the current scope.",
                    Environment,
                    variableDefinition.Line
                );
            }

            var dynValue = Environment.LocalScope.Set(identifier, DynValue.Zero);

            if (variableDefinition.Initializer != null)
            {
                dynValue.CopyFrom(
                    Visit(variableDefinition.Initializer)
                );
            }
        }
    }
}