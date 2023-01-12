using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(VariableDefinition variableDefinition)
        {
            var scope = Environment.LocalScope ?? Environment.GlobalScope;
            
            foreach (var kvp in variableDefinition.Definitions)
            {
                var identifier = kvp.Key;
                var initializer = kvp.Value;

                DynValue dynValue;

                if (scope.HasMember(identifier))
                {
                    throw new RuntimeException(
                        $"Variable '{identifier}' was already defined in the current scope.",
                        Environment,
                        variableDefinition.Line
                    );
                }

                dynValue = scope.Set(identifier, DynValue.Zero);

                if (initializer != null)
                {
                    dynValue.CopyFrom(
                        Visit(initializer)
                    );
                }
            }
        }
    }
}