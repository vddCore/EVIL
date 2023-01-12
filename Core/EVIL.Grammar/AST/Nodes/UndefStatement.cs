namespace EVIL.Grammar.AST.Nodes
{
    public class UndefStatement : Statement
    {
        public Expression Right { get; }

        public UndefStatement(Expression right)
        {
            Right = right;
            Reparent(Right);
        }
    }
}
