namespace EVIL.Grammar.AST.Constants;

using EVIL.Grammar.AST.Base;

public sealed class StringConstant(
    string value,
    bool isInterpolated
) : ConstantExpression
{
    public string Value { get; } = value;
    public bool IsInterpolated { get; } = isInterpolated;
}