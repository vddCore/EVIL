namespace EVIL.Grammar.AST.Nodes
{
    public class ConstantNumber : Expression
    {
        public double Value { get; }

        public ConstantNumber(double value)
        {
            Value = value;
        }
    }
}