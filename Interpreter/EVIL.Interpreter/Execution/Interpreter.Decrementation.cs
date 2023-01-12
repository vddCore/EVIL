using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(DecrementationExpression decrementationExpression)
        {
            if (!(decrementationExpression.Target is VariableReferenceExpression) && !(decrementationExpression.Target is IndexerExpression))
            {
                throw new RuntimeException(
                    "A variable value is required as decrement operand",
                    Environment,
                    decrementationExpression.Line
                );
            }

            var numValue = Visit(decrementationExpression.Target);

            if (numValue.Type == DynValueType.Number)
            {
                if (decrementationExpression.IsPrefix)
                {
                    var retVal = new DynValue(numValue.Number - 1);
                    numValue.CopyFrom(retVal);

                    return retVal;
                }
                else
                {
                    var retVal = numValue.Copy();
                    numValue.CopyFrom(new DynValue(numValue.Number - 1));

                    return retVal;
                }
            }

            throw new RuntimeException(
                "Cannot decrement this value because it's not a number.",
                Environment,
                decrementationExpression.Line
            );
        }
    }
}