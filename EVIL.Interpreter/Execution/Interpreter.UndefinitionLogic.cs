using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UndefNode undefNode)
        {
            var variable = undefNode.Variable as VariableNode;

            if (variable == null)
            {
                throw new RuntimeException("Expected a variable.", undefNode.Line);
            }
            
            Environment.LocalScope.UnSetInChain(variable.Identifier);
            return DynValue.Zero;
        }
    }
}
