namespace EVIL.Grammar.AST.Nodes
{
    public class NumberNode : AstNode
    {
        public double Value { get; }

        public NumberNode(double value)
        {
            Value = value;
        }
    }
}