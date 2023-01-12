using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class PostDecrementationNode : AstNode
    {
        public AstNode Left { get; }

        public PostDecrementationNode(AstNode left)
        {
            Left = left;
        }
    }
}
