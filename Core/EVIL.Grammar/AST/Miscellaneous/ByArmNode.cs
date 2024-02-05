using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class ByArmNode : AstNode
    {
        public Expression Selector { get; }
        public Expression Value { get; }
        public bool DeepEquality { get; }

        public ByArmNode(Expression selector, Expression value, bool deepEquality)
        {
            Selector = selector;
            Value = value;
            DeepEquality = deepEquality;

            Reparent(Selector, Value);
        }
    }
}