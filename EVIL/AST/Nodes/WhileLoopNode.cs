using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
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
