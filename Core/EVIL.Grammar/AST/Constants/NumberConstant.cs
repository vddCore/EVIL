namespace EVIL.Grammar.AST.Constants
{
    public sealed class NumberConstant : ConstantExpression
    {
        public double Value { get; }

        public NumberConstant(double value)
        {
            Value = value;
        }
    }
}