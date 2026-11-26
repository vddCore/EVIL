namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class SelfInvocationExpression : Expression
{
    public Expression Indexable { get; }
    public IdentifierNode Identifier { get; }
    public ArgumentList ArgumentList { get; }
    public bool IsConditional { get; }

    public SelfInvocationExpression(
        Expression indexable, 
        IdentifierNode identifier, 
        ArgumentList argumentList,
        bool isConditional = false)
    {
        Indexable = indexable;
        Identifier = identifier;
        ArgumentList = argumentList;
        IsConditional = isConditional;

        Reparent(Indexable, Identifier, ArgumentList);
    }
}
