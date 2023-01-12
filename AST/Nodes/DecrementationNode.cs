using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class DecrementationNode : AstNode
    {
        public AstNode Target { get; }
        public bool IsPrefix { get; }

        public DecrementationNode(AstNode target, bool isPrefix)
        {
            Target = target;
            IsPrefix = isPrefix;
        }
    }
}
