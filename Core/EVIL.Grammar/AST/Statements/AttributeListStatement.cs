using System.Collections.Generic;

namespace EVIL.Grammar.AST.Statements
{
    public class AttributeListStatement : Statement
    {
        public List<AttributeStatement> Attributes { get; }

        public AttributeListStatement(List<AttributeStatement> attributes)
        {
            Attributes = attributes;
            Reparent(Attributes);
        }
    }
}