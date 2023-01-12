using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class IndexingNode : AstNode
    {
        public AstNode Indexable { get; }
        public AstNode KeyExpression { get; }

        public IndexingNode(AstNode indexable, AstNode keyExpression)
        {
            Indexable = indexable;
            KeyExpression = keyExpression;
        }
    }
}
