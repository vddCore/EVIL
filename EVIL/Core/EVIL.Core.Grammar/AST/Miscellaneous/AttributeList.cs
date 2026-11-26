namespace EVIL.Core.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;

public sealed class AttributeList : AstNode
{
    public List<AttributeNode> Attributes { get; }

    public AttributeList(List<AttributeNode> attributes)
    {
        Attributes = attributes;
        Reparent(Attributes);
    }
}