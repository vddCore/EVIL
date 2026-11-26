namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

public sealed class ConditionalExpression : Expression
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