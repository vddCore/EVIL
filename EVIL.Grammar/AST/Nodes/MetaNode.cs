namespace EVIL.Grammar.AST.Nodes
{
    public class MetaNode : AstNode
    {
        public AstNode Left { get; }
        public string Identifier { get; }

        public MetaNode(AstNode left, string identifier)
        {
            Left = left;
            Identifier = identifier;
        }
    }
}