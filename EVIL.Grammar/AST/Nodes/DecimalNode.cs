namespace EVIL.Grammar.AST.Nodes
{
    public class DecimalNode : AstNode
    {
        public decimal Value { get; }

        public DecimalNode(decimal number)
        {
            Value = number;
        }
    }
}