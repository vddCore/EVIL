using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements.TopLevel
{
    public sealed class FnStatement : Statement
    {
        public IdentifierNode Identifier { get; }
        public ParameterList ParameterList { get; }
        public Statement Statement { get; }
        public List<AttributeNode> Attributes { get; }
        public bool IsLocalDefintion { get; }

        public FnStatement(
            IdentifierNode identifier,
            ParameterList parameterList,
            Statement statement,
            List<AttributeNode> attributes,
            bool isLocalDefintion)
        {
            Identifier = identifier;
            ParameterList = parameterList;
            Statement = statement;
            Attributes = attributes;
            IsLocalDefintion = isLocalDefintion;

            Reparent(Identifier);
            Reparent(ParameterList);
            Reparent(Statement);
            Reparent(Attributes);
        }
    }
}