using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public class ExtraArgumentsExpression : Expression
    {
        public bool UnwrapOnStack { get; }

        public ExtraArgumentsExpression(bool unwrapOnStack)
        {
            UnwrapOnStack = unwrapOnStack;
        }
    }
}