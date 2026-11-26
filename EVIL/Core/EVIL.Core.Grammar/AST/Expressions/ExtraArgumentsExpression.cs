namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

public sealed class ExtraArgumentsExpression : Expression
{
    public bool UnwrapOnStack { get; }

    public ExtraArgumentsExpression(bool unwrapOnStack)
    {
        UnwrapOnStack = unwrapOnStack;
    }
}