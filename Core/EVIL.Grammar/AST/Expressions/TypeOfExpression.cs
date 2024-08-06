namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class TypeOfExpression : Expression
{
    public Expression Target { get; }

    public TypeOfExpression(Expression target)
    {
        Target = target;
        Reparent(Target);
    }
}