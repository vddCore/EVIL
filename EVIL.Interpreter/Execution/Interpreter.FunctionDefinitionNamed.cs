using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionDefinitionNamedNode functionDefinitionNamedNode)
        {
            var name = functionDefinitionNamedNode.Identifier;

            var fn = new ScriptFunction(
                functionDefinitionNamedNode.Statements,
                functionDefinitionNamedNode.Parameters,
                functionDefinitionNamedNode.Line
            );

            Environment.RegisterFunction(
                name,
                fn
            );

            foreach (var kvp in Environment.LocalScope.Members)
            {
                fn.Closures.Add(kvp.Key, kvp.Value);
            }

            return DynValue.Zero;
        }
    }
}