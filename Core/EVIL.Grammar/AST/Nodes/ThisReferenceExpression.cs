namespace EVIL.Grammar.AST.Nodes
{
    public class ThisReferenceExpression : Expression
    {
        public IndexerExpression ParentIndexer { get; }

        public ThisReferenceExpression(IndexerExpression parentIndexer)
        {
            ParentIndexer = parentIndexer;
        }
    }
}