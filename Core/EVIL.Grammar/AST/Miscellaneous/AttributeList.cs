using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements.TopLevel
{
    public class AttributeList : AstNode
    {
        public List<AttributeNode> Attributes { get; }

        public AttributeList(List<AttributeNode> attributes)
        {
            Attributes = attributes;
            Reparent(Attributes);
        }
    }
}