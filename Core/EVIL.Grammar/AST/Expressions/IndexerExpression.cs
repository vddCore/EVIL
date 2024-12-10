namespace EVIL.Grammar.AST.Expressions;

using System.Collections.Generic;
using System.Globalization;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

public sealed class IndexerExpression : Expression
{
    public Expression Indexable { get; }
    public Expression KeyExpression { get; }

    public bool IsConditional { get; }
    public bool WillBeAssigned { get; }

    public IndexerExpression(
        Expression indexable, 
        Expression keyExpression, 
        bool isConditional, 
        bool willBeAssigned)
    {
        Indexable = indexable;
        KeyExpression = keyExpression;
            
        IsConditional = isConditional;
        WillBeAssigned = willBeAssigned;

        Reparent(Indexable, KeyExpression);
    }

    public string BuildChainStringRepresentation()
    {
        var stack = new Stack<string>();
        var current = Indexable;
            
        stack.Push(GetKeyStringRepresentation());
            
        while (current is IndexerExpression indexerExpression)
        {
            stack.Push(indexerExpression.GetKeyStringRepresentation());
            current = indexerExpression.Indexable;
        }

        if (current is SymbolReferenceExpression symbolReferenceExpression)
        {
            stack.Push(symbolReferenceExpression.Identifier);
        }

        return string.Join('.', stack);
    }

    public string GetKeyStringRepresentation()
    {
        return KeyExpression switch
        {
            NumberConstant numberConstant => numberConstant.Value.ToString(CultureInfo.InvariantCulture),
            StringConstant stringConstant => stringConstant.Value,
            SymbolReferenceExpression symbolReferenceExpression => symbolReferenceExpression.Identifier,
            _ => "???"
        };
    }
}