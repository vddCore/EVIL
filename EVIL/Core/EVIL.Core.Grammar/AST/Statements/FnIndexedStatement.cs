namespace EVIL.Core.Grammar.AST.Statements;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class FnIndexedStatement : Statement
{
    public IndexerExpression Indexer { get; }
    public AttributeList? AttributeList { get; }
    public ParameterList? ParameterList { get; }
    public Statement InnerStatement { get; }
        
    public FnIndexedStatement(
        IndexerExpression indexer,
        AttributeList? attributeList,
        ParameterList? parameterList,
        Statement innerStatement)
    {
        Indexer = indexer;
        AttributeList = attributeList;
        ParameterList = parameterList;
        InnerStatement = innerStatement;

        Reparent(Indexer);
        Reparent(AttributeList);
        Reparent(ParameterList);
        Reparent(InnerStatement);
    }
}