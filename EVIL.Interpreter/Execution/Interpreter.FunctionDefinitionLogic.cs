using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionDefinitionNode scriptFunctionDefinitionNode)
        {
            var name = scriptFunctionDefinitionNode.Name;

            var fn = new ScriptFunction(
                scriptFunctionDefinitionNode.StatementList,
                scriptFunctionDefinitionNode.ParameterNames,
                scriptFunctionDefinitionNode.Line
            );

            if (name != null)
            {
                if (Environment.LocalScope != Environment.GlobalScope)
                {
                    throw new RuntimeException(
                        "Attempt to define a named function in local scope.",
                        Environment,
                        scriptFunctionDefinitionNode.Line
                    );
                }

                Environment.RegisterFunction(
                    name,
                    fn
                );
            }

            foreach (var kvp in Environment.LocalScope.Members)
            {
                fn.Closures.Add(kvp.Key, kvp.Value);
            }

            return new DynValue(fn);
        }
    }
}