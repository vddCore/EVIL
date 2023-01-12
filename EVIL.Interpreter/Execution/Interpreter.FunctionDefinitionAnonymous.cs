using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionDefinitionAnonymousNode functionDefinitionAnonymousNode)
        {
            var fn = new ScriptFunction(
                functionDefinitionAnonymousNode.Statements,
                functionDefinitionAnonymousNode.Parameters,
                functionDefinitionAnonymousNode.Line
            );

            foreach (var kvp in Environment.LocalScope.Members)
            {
                fn.Closures.Add(kvp.Key, kvp.Value);
            }

            return new DynValue(fn);
        }
    }
}