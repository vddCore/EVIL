using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(VariableDefinition variableDefinition)
        {
            foreach (var kvp in variableDefinition.Definitions)
            {
                var identifier = kvp.Key;
                var initializer = kvp.Value;

                DynValue dynValue;
                if (Environment.LocalScope != null)
                {
                    if (Environment.LocalScope.HasMember(identifier))
                    {
                        throw new RuntimeException(
                            $"Variable '{identifier}' was already defined in the current scope.",
                            Environment,
                            variableDefinition.Line
                        );
                    }

                    dynValue = Environment.LocalScope.Set(identifier, DynValue.Zero);
                }
                else
                {
                    if (Environment.GlobalScope.HasMember(identifier))
                    {
                        throw new RuntimeException(
                            $"Variable '{identifier}' was already defined in the current scope.",
                            Environment,
                            variableDefinition.Line
                        );
                    }

                    dynValue = Environment.GlobalScope.Set(identifier, DynValue.Zero);
                }

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