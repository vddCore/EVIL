namespace EVIL.Grammar.AST.Base
{
    public abstract class Expression : AstNode
    {
        public virtual Expression Reduce() => this;
    }
}