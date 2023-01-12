using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(FunctionDefinition functionDefinition)
        {
            var name = functionDefinition.Identifier;

            var fn = new ScriptFunction(
                functionDefinition.Statements,
                functionDefinition.Parameters,
                functionDefinition.Line
            );

            Environment.RegisterFunction(
                name,
                fn
            );

            foreach (var kvp in Environment.LocalScope.Members)
            {
                fn.Closures.Add(kvp.Key, kvp.Value);
            }
        }
    }
}