namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;

public sealed class ExpressionBodyStatement : Statement
{
    public Expression Expression { get; }

    public ExpressionBodyStatement(Expression expression)
    {
        Expression = expression;
        Reparent(Expression);
    }
}