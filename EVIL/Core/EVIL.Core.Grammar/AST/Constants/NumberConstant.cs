namespace EVIL.Core.Grammar.AST.Constants;

using EVIL.Core.Grammar.AST.Base;

public sealed class NumberConstant(double value) 
    : ConstantExpression
{
    public double Value { get; } = value;
}