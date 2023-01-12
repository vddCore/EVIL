using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class IncrementationNode : AstNode
    {
        public AstNode Target { get; }
        public bool IsPrefix { get; }

        public IncrementationNode(AstNode target, bool isPrefix)
        {
            Target = target;
            IsPrefix = isPrefix;
        }
    }
}
