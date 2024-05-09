using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class RetStatement : Statement
    {
        public Expression? Expression { get; }

        public RetStatement(Expression? expression)
        {
            Expression = expression;
            Reparent(Expression);
        }
    }
}
