using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(DecrementationNode decrementationNode)
        {
            if (!(decrementationNode.Target is VariableNode) && !(decrementationNode.Target is IndexingNode))
            {
                throw new RuntimeException(
                    "A variable value is required as decrement operand",
                    Environment,
                    decrementationNode.Line
                );
            }
            
            var numValue = Visit(decrementationNode.Target);

            if (numValue.Type == DynValueType.Decimal)
            {
                if (decrementationNode.IsPrefix)
                {
                    var retVal = new DynValue(numValue.Decimal - 1);
                    numValue.CopyFrom(retVal);
                
                    return retVal;
                }
                else
                {
                    var retVal = numValue.Copy();
                    numValue.CopyFrom(new DynValue(numValue.Decimal - 1));

                    return retVal;
                }
            }
            else if (numValue.Type == DynValueType.Integer)
            {
                if (decrementationNode.IsPrefix)
                {
                    var retVal = new DynValue(numValue.Integer - 1);
                    numValue.CopyFrom(retVal);
                
                    return retVal;
                }
                else
                {
                    var retVal = numValue.Copy();
                    numValue.CopyFrom(new DynValue(numValue.Integer - 1));

                    return retVal;
                }
            }
            
            throw new RuntimeException(
                "Cannot decrement this value because it's not a number.", 
                Environment,
                decrementationNode.Line
            );
        }
    }
}