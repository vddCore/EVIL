using System;
using System.Collections.Generic;
using System.Text;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
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
