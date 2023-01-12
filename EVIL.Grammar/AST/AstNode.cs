using EVIL.Grammar.AST.Nodes;

namespace EVIL.Grammar.AST
{
    public abstract class AstNode
    {      
        public int Line { get; set; }
        public AstNode Parent { get; set; }

        public bool IsConstant =>
            this is NumberNode
            || this is StringNode
            || this is TableNode;

        protected void Reparent(params AstNode[] nodes)
        {
            foreach (var node in nodes)
            {
                node.Parent = this;
            }
        }
    }
}