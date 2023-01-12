using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class WhileLoopNode : AstNode
    {
        public AstNode Expression { get; }
        public List<AstNode> StatementList { get; }

        public WhileLoopNode(AstNode expression, List<AstNode> statementList)
        {
            Expression = expression;
            StatementList = statementList;
        }
    }
}
