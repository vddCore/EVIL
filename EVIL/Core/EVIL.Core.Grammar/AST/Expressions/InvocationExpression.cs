namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

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
