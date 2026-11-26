namespace EVIL.Core.Grammar.AST.Base;

using EVIL.Core.Grammar.AST.Expressions;

public abstract class Expression : AstNode
{
    public bool IsValidExpressionStatement
        => this is AssignmentExpression
            or InvocationExpression
            or SelfInvocationExpression
            or IncrementationExpression
            or DecrementationExpression
            or YieldExpression
            or NonNilAssertionExpression;
        
    public virtual Expression Reduce() => this;
}