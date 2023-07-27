using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class AttributeNode : AstNode
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
            Reparent(values);
            Reparent(Properties.Keys);
            Reparent(Properties.Values);
        }
    }
}