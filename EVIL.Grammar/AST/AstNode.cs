using EVIL.Grammar.AST.Nodes;

namespace EVIL.Grammar.AST
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public AstNode Parent { get; set; }

        public bool IsConstant =>
            this is NumberExpression
            || this is StringConstant
            || this is TableExpression;

        protected void Reparent(params AstNode[] nodes)
        {
            for (var i = 0; i < nodes.Length; i++)
                nodes[i].Parent = this;
        }
    }
}