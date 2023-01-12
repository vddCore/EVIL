using System.Linq;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionExpression functionExpression)
        {
            var fn = new ScriptFunction(
                functionExpression.Statements,
                functionExpression.Parameters,
                functionExpression.Line
            );

            var scope = Environment.LocalScope;
            
            while (scope != null)
            {
                foreach (var kvp in scope.Members)
                {
                    fn.Closures.Add(kvp.Key, kvp.Value);
                }
                
                scope = scope.ParentScope;
            }
            
            return new DynValue(fn);
        }
    }
}