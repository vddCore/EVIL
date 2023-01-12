namespace EVIL.Grammar.AST.Nodes
{
    public class BinaryOperationNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }
        
        public BinaryOperationType Type { get; }

        public BinaryOperationNode(AstNode left, AstNode right, BinaryOperationType type)
        {
            Left = left;
            Right = right;
            
            Type = type;
        }
    }
}