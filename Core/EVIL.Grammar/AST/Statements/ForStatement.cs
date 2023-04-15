using System.Collections.Generic;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class ForStatement : Statement
    {
        public List<Statement> Assignments { get; }
        public Expression Condition { get; }
        public List<Expression> IterationExpressions { get; }
        public Statement Statement { get; }

        public ForStatement(
            List<Statement> assignments, 
            Expression condition, 
            List<Expression> iterationExpressions,
            Statement statement)
        {
            Assignments = assignments;
            Condition = condition;
            IterationExpressions = iterationExpressions;
            Statement = statement;

            for (var i = 0; i < Assignments.Count; i++)
                Reparent(Assignments[i]);

            for (var i = 0; i < IterationExpressions.Count; i++)
                Reparent(IterationExpressions[i]);

            Reparent(Condition);
            Reparent(Statement);
        }
    }
}