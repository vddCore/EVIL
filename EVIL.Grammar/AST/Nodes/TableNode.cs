using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class TableNode : AstNode
    {
        public List<AstNode> Initializers { get; }
        public bool Keyed { get; }

        public TableNode(List<AstNode> initializers, bool keyed)
        {
            Initializers = initializers;
            Keyed = keyed;
            
            Reparent(initializers.ToArray());
        }
    }
}
