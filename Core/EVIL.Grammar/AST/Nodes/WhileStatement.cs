namespace EVIL.Grammar.AST.Nodes
{
    public class WhileStatement : Statement
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
