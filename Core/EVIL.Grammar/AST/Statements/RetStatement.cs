namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;

public sealed class RetStatement : Statement
{
    public Expression? Expression { get; }

    public RetStatement(Expression? expression)
    {
        Expression = expression;
        Reparent(Expression);
    }
}