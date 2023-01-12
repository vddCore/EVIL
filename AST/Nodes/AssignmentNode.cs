using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class AssignmentNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }
        public bool LocalScope { get; }

        public AssignmentNode(AstNode left, AstNode right, bool localScope)
        {
            Left = left;
            Right = right;
            
            LocalScope = localScope;
        }
    }
}