using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class FnIndexedStatement : Statement
    {
        public IndexerExpression Indexer { get; }
        public ParameterList? ParameterList { get; }
        public Statement Statement { get; }
        public List<AttributeNode> Attributes { get; }
        
        public FnIndexedStatement(
            IndexerExpression indexer,
            ParameterList? parameterList,
            Statement statement,
            List<AttributeNode> attributes)
        {
            Indexer = indexer;
            ParameterList = parameterList;
            Statement = statement;
            Attributes = attributes;

            Reparent(Indexer);
            Reparent(ParameterList);
            Reparent(Statement);
            Reparent(Attributes);
        }
    }
}