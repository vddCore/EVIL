namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

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