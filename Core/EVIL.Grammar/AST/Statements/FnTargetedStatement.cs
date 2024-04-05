using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class FnTargetedStatement : Statement
    {
        public IdentifierNode PrimaryIdentifier { get; }
        public IdentifierNode SecondaryIdentifier { get; }
        public ParameterList ParameterList { get; }
        public Statement Statement { get; }
        public List<AttributeNode> Attributes { get; }
        
        public FnTargetedStatement(
            IdentifierNode primaryIdentifier,
            IdentifierNode secondaryIdentifier,
            ParameterList parameterList,
            Statement statement,
            List<AttributeNode> attributes)
        {
            PrimaryIdentifier = primaryIdentifier;
            SecondaryIdentifier = secondaryIdentifier;
            ParameterList = parameterList;
            Statement = statement;
            Attributes = attributes;

            Reparent(PrimaryIdentifier);
            Reparent(SecondaryIdentifier);
            Reparent(ParameterList);
            Reparent(Statement);
            Reparent(Attributes);
        }
    }
}