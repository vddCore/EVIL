namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class TypeOfExpression : Expression
{
    public Expression Target { get; }
    public bool IsNative { get; }

    public TypeOfExpression(Expression target, bool isNative)
    {
        Target = target;
        IsNative = isNative;
        
        Reparent(Target);
    }
}