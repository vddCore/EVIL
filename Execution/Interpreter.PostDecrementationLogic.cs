using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(PostDecrementationNode postDecrementationNode)
        {
            DynValue retVal;
            var name = postDecrementationNode.Variable.Name;
            var numValue = Environment.LocalScope.FindInScopeChain(name);

            if (numValue == null)
            {
                throw new RuntimeException(
                    $"'{name} could not be found in the current scope.", postDecrementationNode.Line
                );
            }

            retVal = numValue.Copy();
            numValue.CopyFrom(new DynValue(retVal.Number - 1));

            return retVal;
        }
    }
}