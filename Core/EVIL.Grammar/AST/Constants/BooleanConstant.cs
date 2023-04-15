namespace EVIL.Grammar.AST.Constants
{
    public sealed class BooleanConstant : ConstantExpression
    {
        public bool Value { get; }

        public BooleanConstant(bool value)
        {
            Value = value;
        }
    }
}