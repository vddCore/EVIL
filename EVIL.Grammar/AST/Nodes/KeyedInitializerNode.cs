namespace EVIL.Grammar.AST.Nodes
{
    public class KeyedInitializerNode : AstNode
    {
        public AstNode Key { get; }
        public AstNode Value { get; }

        public KeyedInitializerNode(AstNode key, AstNode value)
        {
            Key = key;
            Value = value;
        }
    }
}