using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ForStatement : Statement
    {
        public List<AstNode> Assignments { get; }
        public Expression Condition { get; }
        public List<Expression> IterationExpressions { get; }
        public Statement Statement { get; }

        public ForStatement(
            List<AstNode> assignments, 
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