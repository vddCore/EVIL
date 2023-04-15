namespace EVIL.Grammar.AST
{
    public abstract class Expression : AstNode
    {
        public virtual Expression Reduce() => this;
    }
}