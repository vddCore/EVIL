namespace EVIL.Grammar.AST.Expressions
{
    public sealed class DecrementationExpression : Expression
    {
        public Expression Target { get; }
        public bool IsPrefix { get; }

        public DecrementationExpression(Expression target, bool isPrefix)
        {
            target.Parent = this;
            
            Target = target;
            IsPrefix = isPrefix;
        }
    }
}
