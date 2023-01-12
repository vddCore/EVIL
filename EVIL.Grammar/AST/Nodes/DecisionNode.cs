using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class DecisionNode : AstNode
    {
        public List<AstNode> Conditions { get; } = new();
        public List<AstNode> Statements { get; } = new();

        public AstNode ElseBranch { get; internal set; }
    }
}
