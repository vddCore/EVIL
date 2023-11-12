using EVIL.Grammar.AST.Expressions;

namespace EVIL.Grammar.AST.Base
{
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
}