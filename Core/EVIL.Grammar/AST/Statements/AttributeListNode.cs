using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements.TopLevel
{
    public class AttributeListNode : Statement
    {
        public List<AttributeNode> Attributes { get; }

        public AttributeListNode(List<AttributeNode> attributes)
        {
            Attributes = attributes;
            Reparent(Attributes);
        }
    }
}