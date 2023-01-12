using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class FunctionCallNode : AstNode
    {
        public AstNode Left { get; }
        public List<AstNode> Parameters { get; }

        public FunctionCallNode(AstNode left, List<AstNode> parameters)
        {
            Left = left;
            Parameters = parameters;
        }
    }
}