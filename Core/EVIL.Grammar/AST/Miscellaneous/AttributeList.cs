namespace EVIL.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class AttributeList : AstNode
{
    public List<AttributeNode> Attributes { get; }

    public AttributeList(List<AttributeNode> attributes)
    {
        Attributes = attributes;
        Reparent(Attributes);
    }
}