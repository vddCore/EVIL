using EVIL.Grammar.AST.Base;

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