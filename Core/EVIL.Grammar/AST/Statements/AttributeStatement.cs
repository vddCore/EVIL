using System.Collections.Generic;

namespace EVIL.Grammar.AST.Statements
{
    public class AttributeStatement : Statement
    {
        public string Name { get; }
        public List<AstNode> Values { get; }

        public AttributeStatement(string name, List<AstNode> values)
        {
            Name = name;
            Values = values;

            Reparent(values);
        }
    }
}