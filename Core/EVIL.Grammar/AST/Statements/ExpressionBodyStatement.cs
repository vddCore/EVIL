namespace EVIL.Grammar.AST.Statements
{
    public class ExpressionBodyStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionBodyStatement(Expression expression)
        {
            Expression = expression;
            Reparent(Expression);
        }
    }
}