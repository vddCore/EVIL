namespace EVIL.Grammar.AST.Nodes
{
    public class IncrementationExpression : Expression
    {
        public Expression Target { get; }
        public bool IsPrefix { get; }

        public IncrementationExpression(Expression target, bool isPrefix)
        {
            Target = target;
            IsPrefix = isPrefix;

            Reparent(Target);
        }
    }
}
