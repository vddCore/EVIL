namespace EVIL.Grammar.AST.Base;

using EVIL.Grammar.AST.Expressions;

public abstract class Expression : AstNode
{
    public bool IsValidExpressionStatement
        => this is AssignmentExpression
            or InvocationExpression
            or SelfInvocationExpression
            or IncrementationExpression
            or DecrementationExpression
            or YieldExpression;
        
    public virtual Expression Reduce() => this;
}