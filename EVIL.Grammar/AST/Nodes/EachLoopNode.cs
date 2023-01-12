namespace EVIL.Grammar.AST.Nodes
{
    public class EachLoopNode : AstNode
    {
        public AstNode KeyNode { get; }
        public AstNode ValueNode { get; }
        public AstNode TableNode { get; }
        public BlockStatementNode Statements { get; }

        public EachLoopNode(AstNode keyNode, AstNode valueNode, AstNode tableNode, BlockStatementNode statements)
        {
            KeyNode = keyNode;
            ValueNode = valueNode;
            TableNode = tableNode;

            Statements = statements;
        }
    }
}