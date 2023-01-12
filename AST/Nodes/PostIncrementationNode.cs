using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class PostIncrementationNode : AstNode
    {
        public VariableNode Variable { get; }

        public PostIncrementationNode(VariableNode variable)
        {
            Variable = variable;
        }
    }
}
