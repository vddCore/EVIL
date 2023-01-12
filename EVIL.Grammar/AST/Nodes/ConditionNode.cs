using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ConditionNode : AstNode
    {
        public Dictionary<AstNode, AstNode> IfElifBranches { get; } = new();
        public AstNode ElseBranch { get; set; }
    }
}
