using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class AttributeNode : AstNode
    {
        public string Name { get; }
        
        public List<AstNode> Values { get; }
        public Dictionary<string, AstNode> Properties { get; }

        public AttributeNode(string name, List<AstNode> values, Dictionary<string, AstNode> properties)
        {
            Name = name;
            Values = values;
            Properties = properties;

            Reparent(values);
            Reparent(Properties.Values);
        }
    }
}