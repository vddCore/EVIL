using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ForLoopNode : AstNode
    {
        public List<AstNode> Assignments { get; }
        public AstNode Condition { get; }
        public List<AstNode> IterationStatements { get; }
        
        public List<AstNode> StatementList { get; }

        public ForLoopNode(List<AstNode> assignments, AstNode condition, List<AstNode> iterationStatements, List<AstNode> statementList)
        {
            Assignments = assignments;
            Condition = condition;
            IterationStatements = iterationStatements;
            
            StatementList = statementList;
        }
    }
}
