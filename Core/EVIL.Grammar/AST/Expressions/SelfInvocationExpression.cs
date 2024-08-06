namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public sealed class SelfInvocationExpression : Expression
{
    public Expression Indexable { get; }
    public IdentifierNode Identifier { get; }
    public ArgumentList ArgumentList { get; }
        
    public SelfInvocationExpression(
        Expression indexable, 
        IdentifierNode identifier, 
        ArgumentList argumentList)
    {
        Indexable = indexable;
        Identifier = identifier;
        ArgumentList = argumentList;

        Reparent(Indexable, Identifier, ArgumentList);
    }
}