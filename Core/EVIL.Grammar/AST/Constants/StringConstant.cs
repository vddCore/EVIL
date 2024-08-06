namespace EVIL.Grammar.AST.Constants;

using EVIL.Grammar.AST.Base;

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