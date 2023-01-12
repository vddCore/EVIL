using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class ReturnNode : AstNode
    {
        public AstNode Expression { get; }

        public ReturnNode(AstNode expression)
        {
            Expression = expression;
        }
    }
}
