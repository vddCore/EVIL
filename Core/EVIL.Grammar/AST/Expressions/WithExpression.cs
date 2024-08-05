using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public class WithExpression : Expression
    {
        public Expression BaseExpression { get; }
        public TableExpression TableExpansionExpression { get; }

        public WithExpression(Expression baseExpression, TableExpression tableExpansionExpression)
        {
            BaseExpression = baseExpression;
            TableExpansionExpression = tableExpansionExpression;

            Reparent(BaseExpression, TableExpansionExpression);
        }
    }
}