namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public sealed class InvocationExpression : Expression
{
    public Expression Callee { get; }
    public ArgumentList ArgumentList { get; }
    public bool IsConditional { get; }

    public InvocationExpression(
        Expression callee,
        ArgumentList argumentList,
        bool isConditional = false)
    {
        Callee = callee;
        ArgumentList = argumentList;
        IsConditional = isConditional;

        Reparent(Callee);
        Reparent(ArgumentList);
    }
}
