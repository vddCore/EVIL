namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

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
