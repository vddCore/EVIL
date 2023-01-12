using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ConditionalExpressionNode conditionalExpressionNode)
        {
            var conditionValue = Visit(conditionalExpressionNode.Condition);

            if (conditionValue.IsTruth)
            {
                return Visit(conditionalExpressionNode.TrueExpression);
            }

            return Visit(conditionalExpressionNode.FalseExpression);
        }
    }
}