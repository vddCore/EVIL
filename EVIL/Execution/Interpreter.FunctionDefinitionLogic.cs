using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionDefinitionNode scriptFunctionDefinitionNode)
        {
            var name = scriptFunctionDefinitionNode.Name;

            var fn = new ScriptFunction(
                scriptFunctionDefinitionNode.StatementList,
                scriptFunctionDefinitionNode.ParameterNames
            );

            if (name != null)
            {
                if (Environment.LocalScope != Environment.GlobalScope)
                {
                    throw new RuntimeException("Attempt to define a named function in local scope.",
                        scriptFunctionDefinitionNode.Line);
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