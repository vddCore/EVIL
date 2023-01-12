using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class IfStatement : Statement
    {
        private List<Expression> _conditions = new();
        public List<Statement> _statements = new();
        
        public IReadOnlyList<Expression> Conditions => _conditions;
        public IReadOnlyList<Statement> Statements => _statements;

        public Statement ElseBranch { get; private set; }

        public void AddCondition(Expression expr)
        {
            Reparent(expr);
            _conditions.Add(expr);
        }

        public void AddStatement(Statement stmt)
        {
            Reparent(stmt);
            _statements.Add(stmt);
        }

        public void SetElseBranch(Statement stmt)
        {
            Reparent(stmt);
            ElseBranch = stmt;
        }
    }
}
