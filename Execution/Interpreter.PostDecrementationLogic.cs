using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(DecrementationNode decrementationNode)
        {
            var numValue = Visit(decrementationNode.Target);

            if (numValue.Type != DynValueType.Number)
            {
                throw new RuntimeException(
                    "Cannot decrement this value because it's not a number.", decrementationNode.Line
                );
            }

            if (decrementationNode.IsPrefix)
            {
                numValue.CopyFrom(new DynValue(numValue.Number - 1));
                return numValue;
            }
            else
            {
                var retVal = numValue.Copy();
                numValue.CopyFrom(new DynValue(numValue.Number - 1));

                return retVal;
            }
        }
    }
}