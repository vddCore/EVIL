using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public class TypeOfExpression : Expression
    {
        public Expression Target { get; }

        public TypeOfExpression(Expression target)
        {
            Target = target;
            Reparent(Target);
        }
    }
}