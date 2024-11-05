namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

public sealed class IsExpression : Expression
{
    public Expression Target { get; }
    public TypeCodeConstant Type { get; }
    public StringConstant? ManagedTypeName { get; }
    public bool Invert { get; }
    
    public bool IsManagedTypeCheck => ManagedTypeName is not null;

    public IsExpression(Expression target, TypeCodeConstant type, StringConstant? managedTypeName, bool invert)
    {
        Target = target;
        Type = type;
        ManagedTypeName = managedTypeName;
        Invert = invert;

        Reparent(Target, Type, ManagedTypeName);
    }
}