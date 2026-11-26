namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

public sealed class IncrementationExpression : Expression
{
    public Expression Target { get; }
    public bool IsPrefix { get; }

    public IncrementationExpression(Expression target, bool isPrefix)
    {
        Target = target;
        IsPrefix = isPrefix;

        Reparent(Target);
    }
}