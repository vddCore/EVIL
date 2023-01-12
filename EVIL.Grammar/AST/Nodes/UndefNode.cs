namespace EVIL.Grammar.AST.Nodes
{
    public class UndefNode : AstNode
    {
        public AstNode Symbol { get; }

        public UndefNode(AstNode symbol)
        {
            Symbol = symbol;
        }
    }
}
