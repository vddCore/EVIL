namespace EVIL.Grammar.AST.Constants;

using EVIL.Grammar.AST.Base;

public sealed class NumberConstant(double value) 
    : ConstantExpression
{
    public double Value { get; } = value;
}