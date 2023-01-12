using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
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
                var retVal = new DynValue(numValue.Number - 1);
                numValue.CopyFrom(retVal, true);
                
                return retVal;
            }
            else
            {
                var retVal = numValue.Copy();
                numValue.CopyFrom(new DynValue(numValue.Number - 1), true);

                return retVal;
            }
        }
    }
}