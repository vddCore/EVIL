namespace EVIL.Grammar.AST.Nodes
{
    public class IntegerNode : AstNode
    {
        public int Value { get; }

        public IntegerNode(int value)
        {
            Value = value;
        }
    }
}