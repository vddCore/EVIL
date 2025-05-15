namespace EVIL.Grammar.AST.Constants;

using EVIL.Grammar.AST.Base;

public sealed class BooleanConstant(bool value) 
    : ConstantExpression
{
    public bool Value { get; } = value;
}