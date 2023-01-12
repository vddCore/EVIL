using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ForLoopNode : AstNode
    {
        public List<AstNode> Assignments { get; }
        public AstNode Condition { get; }
        public List<AstNode> IterationStatements { get; }
        
        public BlockStatementNode Statements { get; }

        public ForLoopNode(List<AstNode> assignments, AstNode condition, List<AstNode> iterationStatements, BlockStatementNode statements)
        {
            Assignments = assignments;
            Condition = condition;
            IterationStatements = iterationStatements;

            Statements = statements;
        }
    }
}
