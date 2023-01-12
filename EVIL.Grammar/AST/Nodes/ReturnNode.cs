namespace EVIL.Grammar.AST.Nodes
{
    public class ReturnNode : AstNode
    {
        public AstNode Right { get; }

        public ReturnNode(AstNode right)
        {
            Right = right;
        }
    }
}
