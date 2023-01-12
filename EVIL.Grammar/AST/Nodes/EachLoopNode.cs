namespace EVIL.Grammar.AST.Nodes
{
    public class EachLoopNode : AstNode
    {
        public AstNode KeyNode { get; }
        public AstNode ValueNode { get; }
        public AstNode TableNode { get; }
        public AstNode Statement { get; }

        public EachLoopNode(AstNode keyNode, AstNode valueNode, AstNode tableNode, AstNode statement)
        {
            KeyNode = keyNode;
            ValueNode = valueNode;
            TableNode = tableNode;
            Statement = statement;

            Reparent(KeyNode, ValueNode, TableNode, Statement);
        }
    }
}