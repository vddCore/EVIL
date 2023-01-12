using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class RootNode : AstNode
    {
        public List<AstNode> Children { get; }

        public RootNode()
        {
            Children = new List<AstNode>();
        }
    }
}