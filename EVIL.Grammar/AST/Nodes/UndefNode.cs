namespace EVIL.Grammar.AST.Nodes
{
    public class UndefNode : AstNode
    {
        public AstNode Right { get; }

        public UndefNode(AstNode right)
        {
            Right = right;

            Reparent(Right);
        }
    }
}
