namespace EVIL.Grammar.AST.Expressions
{
    public sealed class AssignmentExpression : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        
        public AssignmentOperationType OperationType { get; }

        public AssignmentExpression(Expression left, Expression right, AssignmentOperationType operationType)
        {
            Left = left;
            Right = right;

            OperationType = operationType;

            Reparent(Left, Right);
        }
    }
}