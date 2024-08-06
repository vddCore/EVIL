namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;

public sealed class ThrowStatement : Statement
{
    public Expression ThrownExpression { get; }

    public ThrowStatement(Expression thrownExpression)
    {
        ThrownExpression = thrownExpression;
        Reparent(ThrownExpression);
    }
}