using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(PostIncrementationNode postIncrementationNode)
        {
            DynValue retVal;
            var name = postIncrementationNode.Variable.Name;
            var numValue = Environment.LocalScope.FindInScopeChain(name);

            if (numValue == null)
            {
                throw new RuntimeException(
                    $"'{name} could not be found in the current scope.", postIncrementationNode.Line
                );
            }

            retVal = numValue.Copy();
            numValue.CopyFrom(new DynValue(retVal.Number + 1));

            return retVal;
        }
    }
}