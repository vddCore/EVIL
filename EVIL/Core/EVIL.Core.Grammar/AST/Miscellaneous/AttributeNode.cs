namespace EVIL.Core.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;

public sealed class AttributeNode : AstNode
{
    public IdentifierNode Identifier { get; }
        
    public List<AstNode> Values { get; }
    public Dictionary<IdentifierNode, AstNode> Properties { get; }

    public AttributeNode(
        IdentifierNode identifier,
        List<AstNode> values,
        Dictionary<IdentifierNode, AstNode> properties)
    {
        Identifier = identifier;
        Values = values;
        Properties = properties;

        Reparent(Identifier);
        Reparent(Values);
        Reparent(Properties.Keys);
        Reparent(Properties.Values);
    }
}