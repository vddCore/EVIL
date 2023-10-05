using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class InvocationExpression : Expression
    {
        public Expression Callee { get; }
        public ArgumentList ArgumentList { get; }

        public InvocationExpression(Expression callee, ArgumentList argumentList)
        {
            Callee = callee;
            ArgumentList = argumentList;

            Reparent(Callee);
            Reparent(ArgumentList);
        }
    }
}