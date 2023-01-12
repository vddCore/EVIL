using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(PostIncrementationNode postIncrementationNode)
        {
            var numValue = Visit(postIncrementationNode.Left);

            if (numValue.Type != DynValueType.Number)
            {
                throw new RuntimeException(
                    "Cannot increment this value because it's not a number.", postIncrementationNode.Line
                );
            }
            
            var retVal = numValue.Copy();
            numValue.CopyFrom(new DynValue(numValue.Number + 1));

            return retVal;
        }
    }
}