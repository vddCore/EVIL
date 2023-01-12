namespace EVIL.Grammar.AST.Nodes
{
    public class UnaryOperationNode : AstNode
    {
        public AstNode Right { get; }
        public UnaryOperationType Type { get; }

        public UnaryOperationNode(AstNode right, UnaryOperationType type)
        {
            Right = right;
            Type = type;
        }
    }
}