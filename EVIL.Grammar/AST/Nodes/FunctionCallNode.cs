using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionCallNode : AstNode
    {
        public AstNode Left { get; }
        public List<AstNode> Parameters { get; }

        public FunctionCallNode(AstNode left, List<AstNode> parameters)
        {
            Left = left;
            Parameters = parameters;

            Reparent(left);
            Reparent(Parameters.ToArray());
        }
    }
}