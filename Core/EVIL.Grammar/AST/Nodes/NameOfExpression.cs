namespace EVIL.Grammar.AST.Nodes
{
    public class NameOfExpression : Expression
    {
        public Expression Right { get; }

        public NameOfExpression(Expression right)
        {
            Right = right;
            
            Reparent(Right);
        }
    }
}