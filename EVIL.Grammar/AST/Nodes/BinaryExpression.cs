namespace EVIL.Grammar.AST.Nodes
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        
        public BinaryOperationType Type { get; }

        public BinaryExpression(Expression left, Expression right, BinaryOperationType type)
        {
            Left = left;
            Right = right;
            
            Type = type;

            Reparent(Left, Right);
        }
    }
}