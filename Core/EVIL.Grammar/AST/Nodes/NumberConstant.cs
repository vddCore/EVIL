namespace EVIL.Grammar.AST.Nodes
{
    public class NumberConstant : Expression
    {
        public double Value { get; }

        public NumberConstant(double value)
        {
            Value = value;
        }
    }
}