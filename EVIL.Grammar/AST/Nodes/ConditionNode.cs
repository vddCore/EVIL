using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
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
