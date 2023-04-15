namespace EVIL.Grammar.AST.Statements
{
    public sealed class WhileStatement : Statement
    {
        public Expression Expression { get; }
        public Statement Statement { get; }

        public WhileStatement(Expression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;

            Reparent(Expression, Statement);
        }
    }
}
