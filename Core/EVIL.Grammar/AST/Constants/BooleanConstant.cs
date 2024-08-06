namespace EVIL.Grammar.AST.Constants;

using EVIL.Grammar.AST.Base;

public sealed class BooleanConstant : ConstantExpression
{
    public bool Value { get; }

    public BooleanConstant(bool value)
    {
        Value = value;
    }
}