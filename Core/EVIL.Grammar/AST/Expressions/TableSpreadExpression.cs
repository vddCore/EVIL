namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class TableSpreadExpression : Expression
{
    public Expression Target { get; }
    public bool IsNilTolerant { get; }

    public TableSpreadExpression(Expression target, bool isNilTolerant)
    {
        Target = target;
        IsNilTolerant = isNilTolerant;

        Reparent(Target);
    }
}
