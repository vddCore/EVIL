namespace EVIL.Grammar.AST.Expressions
{
    public sealed class FunctionCallExpression : Expression
    {
        public Expression Callee { get; }
        public ArgumentList ArgumentList { get; }

        public FunctionCallExpression(Expression callee, ArgumentList argumentList)
        {
            Callee = callee;
            ArgumentList = argumentList;

            Reparent(Callee);
            Reparent(ArgumentList);
        }
    }
}