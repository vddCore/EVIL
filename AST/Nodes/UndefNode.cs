using EVIL.AST.Base;
using EVIL.AST.Enums;

namespace EVIL.AST.Nodes
{
    public class UndefNode : AstNode
    {
        public AstNode Variable { get; }

        public UndefNode(AstNode variable)
        {
            Variable = variable;
        }
    }
}
