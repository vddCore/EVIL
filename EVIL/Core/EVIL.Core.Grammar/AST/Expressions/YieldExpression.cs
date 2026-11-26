namespace EVIL.Core.Grammar.AST.Expressions;

using System;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class YieldExpression : Expression
{
    public Expression Invocation { get; }

    public AstNode Target => Invocation switch
    {
        InvocationExpression inv => inv.Callee,
        SelfInvocationExpression self => self.Identifier,
        _ => Invocation
    };

    public ArgumentList ArgumentList => Invocation switch
    {
        InvocationExpression inv => inv.ArgumentList,
        SelfInvocationExpression self => self.ArgumentList,
        _ => throw new InvalidOperationException()
    };

    public YieldExpression(Expression invocation)
    {
        Invocation = invocation;
        Reparent(Invocation);
    }
}