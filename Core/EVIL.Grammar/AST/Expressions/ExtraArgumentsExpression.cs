namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class ExtraArgumentsExpression : Expression
{
    public bool UnwrapOnStack { get; }

    public ExtraArgumentsExpression(bool unwrapOnStack)
    {
        UnwrapOnStack = unwrapOnStack;
    }
}