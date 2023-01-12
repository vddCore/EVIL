using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
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
