namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class WithExpression : Expression
{
    public Expression BaseExpression { get; }
    public TableExpression TableExpansionExpression { get; }

    public WithExpression(Expression baseExpression, TableExpression tableExpansionExpression)
    {
        BaseExpression = baseExpression;
        TableExpansionExpression = tableExpansionExpression;

        Reparent(BaseExpression, TableExpansionExpression);
    }
}