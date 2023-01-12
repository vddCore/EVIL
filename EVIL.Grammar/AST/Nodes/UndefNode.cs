namespace EVIL.Grammar.AST.Nodes
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
