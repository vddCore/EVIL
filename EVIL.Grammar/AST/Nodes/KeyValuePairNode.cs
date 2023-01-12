namespace EVIL.Grammar.AST.Nodes
{
    public class KeyValuePairNode : AstNode
    {
        public AstNode KeyNode { get; }
        public AstNode ValueNode { get; }

        public KeyValuePairNode(AstNode keyNode, AstNode valueNode)
        {
            KeyNode = keyNode;
            ValueNode = valueNode;
        }
    }
}