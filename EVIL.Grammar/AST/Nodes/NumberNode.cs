namespace EVIL.Grammar.AST.Nodes
{
    public class NumberNode : AstNode
    {
        public decimal Value { get; }

        public NumberNode(decimal number)
        {
            Value = number;
        }
    }
}