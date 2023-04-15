namespace EVIL.Grammar.AST.Expressions
{
    public class UnaryExpression : Expression
    {
        public Expression Right { get; }
        public UnaryOperationType Type { get; }

        public UnaryExpression(Expression right, UnaryOperationType type)
        {
            Right = right;
            Type = type;

            Reparent(Right);
        }
    }
}