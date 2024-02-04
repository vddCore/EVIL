using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class ByArm : AstNode
    {
        public Expression Selector { get; }
        public Expression Value { get; }

        public ByArm(Expression selector, Expression value)
        {
            Selector = selector;
            Value = value;

            Reparent(Selector, Value);
        }
    }
}