namespace EVIL.Grammar.AST.Nodes
{
    public class IndexingNode : AstNode
    {
        public AstNode Indexable { get; }
        public AstNode KeyExpression { get; }

        public bool WillBeAssigned { get; }

        public IndexingNode(AstNode indexable, AstNode keyExpression, bool willBeAssigned)
        {
            Indexable = indexable;
            KeyExpression = keyExpression;

            WillBeAssigned = willBeAssigned;
        }
    }
}

