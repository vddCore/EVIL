using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(IncrementationExpression incrementationExpression)
        {
            if (!(incrementationExpression.Target is VariableReference) && !(incrementationExpression.Target is IndexerExpression))
            {
                throw new RuntimeException(
                    "A variable value is required as increment operand",
                    Environment,
                    incrementationExpression.Line
                );
            }

            var numValue = Visit(incrementationExpression.Target);

            if (numValue.Type == DynValueType.Number)
            {
                if (incrementationExpression.IsPrefix)
                {
                    var retVal = new DynValue(numValue.Number + 1);
                    numValue.CopyFrom(retVal);

                    return retVal;
                }
                else
                {
                    var retVal = numValue.Copy();
                    numValue.CopyFrom(new DynValue(numValue.Number + 1));

                    return retVal;
                }
            }

            throw new RuntimeException(
                "Cannot increment this value because it's not a number.",
                Environment,
                incrementationExpression.Line
            );
        }
    }
}