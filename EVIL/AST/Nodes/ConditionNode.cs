using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class ConditionNode : AstNode
    {
        public Dictionary<AstNode, List<AstNode>> IfElifBranches { get; }
        public List<AstNode> ElseBranch { get; set; }

        public ConditionNode()
        {
            IfElifBranches = new Dictionary<AstNode, List<AstNode>>();
        }
    }
}
