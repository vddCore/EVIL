namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;

public sealed class ExpressionStatement : Statement
{
    public readonly Expression Expression;

    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
        Reparent(Expression);

        Line = Expression.Line;
        Column = Expression.Column;
    }
}