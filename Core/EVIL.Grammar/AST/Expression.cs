namespace EVIL.Grammar.AST
{
    public class Expression : AstNode
    {
        public virtual Expression Reduce() => this;
    }
}