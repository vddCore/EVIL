namespace EVIL.Grammar.AST.Expressions;

using System.Collections.Generic;
using System.Globalization;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

public sealed class IndexerExpression : Expression
{
    public Expression Indexable { get; }
    public Expression? KeyExpression { get; }
    public Expression? StartExpression { get; }
    public Expression? EndExpression { get; }

    public bool IsConditional { get; }
    public bool IsRange { get; }
    public bool IsFromEndIndex { get; }
    public bool StartIsFromEnd { get; }
    public bool EndIsFromEnd { get; }
    public bool WillBeAssigned { get; }
    public bool CreateTableIfNonExistent { get; }

    public IndexerExpression(
        Expression indexable, 
        Expression keyExpression, 
        bool isConditional, 
        bool willBeAssigned,
        bool createTableIfNonExistent)
        : this(
            indexable,
            keyExpression,
            null,
            null,
            false,
            false,
            false,
            false,
            isConditional,
            willBeAssigned,
            createTableIfNonExistent)
    {
    }

    public IndexerExpression(
        Expression indexable,
        Expression? keyExpression,
        Expression? startExpression,
        Expression? endExpression,
        bool isRange,
        bool isFromEndIndex,
        bool startIsFromEnd,
        bool endIsFromEnd,
        bool isConditional,
        bool willBeAssigned,
        bool createTableIfNonExistent)
    {
        Indexable = indexable;
        KeyExpression = keyExpression;
        StartExpression = startExpression;
        EndExpression = endExpression;
            
        IsConditional = isConditional;
        IsRange = isRange;
        IsFromEndIndex = isFromEndIndex;
        StartIsFromEnd = startIsFromEnd;
        EndIsFromEnd = endIsFromEnd;
        WillBeAssigned = willBeAssigned;
        CreateTableIfNonExistent = createTableIfNonExistent;

        Reparent(
            Indexable,
            KeyExpression,
            StartExpression,
            EndExpression
        );
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