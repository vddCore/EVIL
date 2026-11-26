namespace EVIL.Core.Grammar.AST.Statements;

using EVIL.Core.Grammar.AST.Base;

public sealed class ExpressionBodyStatement : Statement
{
    public Expression Expression { get; }

    public ExpressionBodyStatement(Expression expression)
    {
        Expression = expression;
        Reparent(Expression);
    }
}