using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class ForLoopNode : AstNode
    {
        public AstNode Assignment { get; }
        public AstNode TargetValue { get; }
        public AstNode Step { get; }

        public List<AstNode> StatementList { get; }

        public ForLoopNode(AstNode assignment, AstNode targetValue, AstNode step, List<AstNode> statementList)
        {
            Assignment = assignment;
            TargetValue = targetValue;
            Step = step;
            StatementList = statementList;
        }
    }
}
