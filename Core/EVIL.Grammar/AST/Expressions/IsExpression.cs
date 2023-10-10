using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Grammar.AST.Expressions
{
    public class IsExpression : Expression
    {
        public Expression Left { get; }
        public TypeCodeConstant Right { get; }

        public IsExpression(Expression left, TypeCodeConstant right)
        {
            Left = left;
            Right = right;

            Reparent(Left, Right);
        }
    }
}