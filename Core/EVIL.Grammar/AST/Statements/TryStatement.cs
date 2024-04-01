using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Statements
{
    public class TryStatement : Statement
    {
        public Statement InnerStatement { get; }

        public TryStatement(Statement innerStatement)
        {
            InnerStatement = innerStatement;
            Reparent(InnerStatement);
        }
    }
}