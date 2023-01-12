namespace EVIL.Grammar.AST.Nodes
{
    public class VarNode : AstNode
    {
        public string Identifier { get; }
        public AstNode Right { get; }

        public VarNode(string identifier, AstNode right)
        {
            Identifier = identifier;
            Right = right;
        }
    }
}