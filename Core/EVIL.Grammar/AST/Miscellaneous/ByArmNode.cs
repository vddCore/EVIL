using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class ByArmNode : AstNode
    {
        public Expression Selector { get; }
        public AstNode ValueArm { get; }
        public bool DeepEquality { get; }

        public ByArmNode(Expression selector, AstNode valueArm, bool deepEquality)
        {
            Selector = selector;
            ValueArm = valueArm;
            DeepEquality = deepEquality;

            Reparent(Selector, valueArm);
        }
    }
}