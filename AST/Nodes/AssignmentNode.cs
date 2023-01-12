using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class AssignmentNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }

        public AssignmentNode(AstNode left, AstNode right)
        {
            Left = left;
            Right = right;
        }
    }
}