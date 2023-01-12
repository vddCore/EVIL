namespace EVIL.Grammar.AST.Nodes
{
    public class ConditionalExpression : Expression
    {
        public Expression Condition { get; }
        
        public Expression TrueExpression { get; }
        public Expression FalseExpression { get; }

        public ConditionalExpression(Expression condition, Expression trueExpression, Expression falseExpression)
        {
            Condition = condition;
            
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;

            Reparent(Condition, TrueExpression, FalseExpression);
        }
    }
}