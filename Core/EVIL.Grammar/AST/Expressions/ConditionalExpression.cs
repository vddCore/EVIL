using System;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class ConditionalExpression : Expression
    {
        public Expression Condition { get; }
        
        public Expression TrueExpression { get; }
        public Expression FalseExpression { get; }

        public override bool CanBeNil => TrueExpression.CanBeNil 
                                      || FalseExpression.CanBeNil;

        public ConditionalExpression(Expression condition, Expression trueExpression, Expression falseExpression)
        {
            Condition = condition;
            
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;

            Reparent(Condition, TrueExpression, FalseExpression);
        }
    }
}