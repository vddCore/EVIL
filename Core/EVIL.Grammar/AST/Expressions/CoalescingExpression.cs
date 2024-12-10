namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;

public sealed class CoalescingExpression : Expression
{
    public Expression Left { get; }
    public AstNode Right { get; }

    public bool IsThrowing => Right is ThrowStatement;

    public CoalescingExpression(Expression left, AstNode right)
    {
        Left = left;
        Right = right;

        Reparent(left, right);
    }
}