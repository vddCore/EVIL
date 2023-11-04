using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public class CoalescingExpression : Expression
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
}