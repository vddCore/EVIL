namespace EVIL.Grammar.AST.Statements.TopLevel;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public sealed class AttributeList : AstNode
{
    public List<AttributeNode> Attributes { get; }

    public AttributeList(List<AttributeNode> attributes)
    {
        Attributes = attributes;
        Reparent(Attributes);
    }
}