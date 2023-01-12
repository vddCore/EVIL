using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class PostIncrementationNode : AstNode
    {
        public AstNode Left { get; }

        public PostIncrementationNode(AstNode left)
        {
            Left = left;
        }
    }
}
