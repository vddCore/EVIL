namespace EVIL.Grammar.AST
{
    public abstract class AstNode
    {      
        public int Line { get; set; }
        public AstNode Parent { get; set; }

        protected void Reparent(params AstNode[] nodes)
        {
            foreach (var node in nodes)
            {
                node.Parent = this;
            }
        }
    }
}