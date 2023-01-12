using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class TableNode : AstNode
    {
        public List<AstNode> Initializers { get; }

        public TableNode(List<AstNode> initializers)
        {
            Initializers = initializers;
        }
    }
}
