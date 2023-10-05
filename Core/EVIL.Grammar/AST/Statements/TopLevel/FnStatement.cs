using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements.TopLevel
{
    public sealed class FnStatement : TopLevelStatement
    {
        public IdentifierNode Identifier { get; }
        public ParameterList ParameterList { get; }
        public Statement Statement { get; }
        public List<AttributeNode> Attributes { get; }

        public FnStatement(
            IdentifierNode identifier,
            ParameterList parameterList,
            Statement statement,
            List<AttributeNode> attributes)
        {
            Identifier = identifier;
            ParameterList = parameterList;
            Statement = statement;
            Attributes = attributes;

            Reparent(Identifier);
            Reparent(ParameterList);
            Reparent(Statement);
            Reparent(Attributes);
        }
    }
}