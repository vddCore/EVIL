using EVIL.Grammar.AST.Expressions;

namespace EVIL.Grammar.AST.Base
{
    public abstract class Expression : AstNode
    {
        public bool IsValidExpressionStatement
            => this is AssignmentExpression
            || this is InvocationExpression
            || this is SelfInvocationExpression
            || this is IncrementationExpression
            || this is DecrementationExpression
            || this is YieldExpression;
        
        public virtual Expression Reduce() => this;
    }
}