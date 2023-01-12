using System.Linq;
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
                scriptFunctionDefinitionNode.Line,
                scriptFunctionDefinitionNode.IsConstructor
            );

            if (!fn.IsConstructor)
            {
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
            }
            else if (!_currentThisContextStack.Any())
            {
                throw new RuntimeException(
                    "Attempt to define a constructor outside a table initializer.",
                    Environment,
                    scriptFunctionDefinitionNode.Line
                );
            }

            foreach (var kvp in Environment.LocalScope.Members)
            {
                fn.Closures.Add(kvp.Key, kvp.Value);
            }
            
            if (_currentThisContextStack.Any())
            {
                if (fn.Closures.ContainsKey("this"))
                {
                    fn.Closures["this"] = _currentThisContextStack.Peek();
                }
                else
                {
                    fn.Closures.Add("this", _currentThisContextStack.Peek());
                }
            }

            return new DynValue(fn);
        }
    }
}