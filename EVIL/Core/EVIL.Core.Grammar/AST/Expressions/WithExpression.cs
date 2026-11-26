namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

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