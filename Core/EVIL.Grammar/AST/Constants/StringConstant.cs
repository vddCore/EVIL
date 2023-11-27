using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Constants
{
    public sealed class StringConstant : ConstantExpression
    {
        public string Value { get; }
        public bool IsInterpolated { get; }

        public StringConstant(string value, bool isInterpolated)
        {
            Value = value;
            IsInterpolated = isInterpolated;
        }
    }
}