namespace EVIL.Grammar.AST.Statements
{
    public sealed class ReturnStatement : Statement
    {
        public Expression? Expression { get; }

        public ReturnStatement(Expression? expression)
        {
            Expression = expression;

            if (Expression != null)
            {
                Reparent(Expression);
            }
        }
    }
}
