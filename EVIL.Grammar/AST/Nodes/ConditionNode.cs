using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ConditionNode : AstNode
    {
        public Dictionary<AstNode, BlockStatementNode> IfElifBranches { get; } = new();
        public BlockStatementNode ElseBranch { get; set; }
    }
}
