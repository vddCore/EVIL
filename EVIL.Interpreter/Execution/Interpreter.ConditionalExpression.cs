using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ConditionalExpression conditionalExpression)
        {
            var conditionValue = Visit(conditionalExpression.Condition);

            if (conditionValue.IsTruth)
            {
                return Visit(conditionalExpression.TrueExpression);
            }

            return Visit(conditionalExpression.FalseExpression);
        }
    }
}