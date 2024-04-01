using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Statements
{
    public class ThrowStatement : Statement
    {
        public Expression ThrownExpression { get; }

        public ThrowStatement(Expression thrownExpression)
        {
            ThrownExpression = thrownExpression;
            Reparent(ThrownExpression);
        }
    }
}