namespace EVIL.Grammar.AST.Constants;

using EVIL.Grammar.AST.Base;

public sealed class NumberConstant : ConstantExpression
{
    public double Value { get; }

    public NumberConstant(double value)
    {
        Value = value;
    }
}