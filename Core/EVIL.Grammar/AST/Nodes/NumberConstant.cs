namespace EVIL.Grammar.AST.Nodes
{
    public class NumberConstant : ConstantExpression
    {
        public double Value { get; }

        public NumberConstant(double value)
        {
            Value = value;
        }
    }
}