namespace EVIL.Grammar.AST.Statements
{
    public sealed class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
            Line = expression.Line;
        }
    }
}