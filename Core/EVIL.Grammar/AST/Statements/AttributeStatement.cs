using System.Collections.Generic;

namespace EVIL.Grammar.AST.Statements
{
    public class AttributeStatement : Statement
    {
        public string Name { get; }
        
        public List<AstNode> Values { get; }
        public Dictionary<string, AstNode> Properties { get; }

        public AttributeStatement(string name, List<AstNode> values, Dictionary<string, AstNode> properties)
        {
            Name = name;
            Values = values;
            Properties = properties;

            Reparent(values);
            Reparent(Properties.Values);
        }
    }
}