namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class CoalescingExpression : Expression
{
    public Expression Left { get; }
    public Expression Right { get; }

    public CoalescingExpression(Expression left, Expression right)
    {
        Left = left;
        Right = right;

        Reparent(left, right);
    }
}