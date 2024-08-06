namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

public sealed class IsExpression : Expression
{
    public Expression Target { get; }
    public TypeCodeConstant Type { get; }
    public bool Invert { get; }

    public IsExpression(Expression target, TypeCodeConstant type, bool invert)
    {
        Target = target;
        Type = type;
        Invert = invert;

        Reparent(Target, Type);
    }
}