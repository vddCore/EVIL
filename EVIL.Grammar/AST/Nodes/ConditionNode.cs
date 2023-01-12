using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ConditionNode : AstNode
    {
        public Dictionary<AstNode, List<AstNode>> IfElifBranches { get; } = new();
        public List<AstNode> ElseBranch { get; set; }
    }
}
