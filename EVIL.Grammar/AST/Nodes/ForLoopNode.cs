using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ForLoopNode : AstNode
    {
        public List<AstNode> Assignments { get; }
        public AstNode Condition { get; }
        public List<AstNode> IterationStatements { get; }
        public AstNode Statement { get; }

        public ForLoopNode(List<AstNode> assignments, AstNode condition, List<AstNode> iterationStatements, AstNode statement)
        {
            Assignments = assignments;
            Condition = condition;
            IterationStatements = iterationStatements;
            Statement = statement;

            Reparent(Assignments.ToArray());
            Reparent(Condition);
            Reparent(IterationStatements.ToArray());
            Reparent(Statement);
        }
    }
}
