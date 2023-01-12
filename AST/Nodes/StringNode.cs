using EVIL.Abstraction;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class StringNode : AstNode
    {
        public DynValue Value { get; }

        public StringNode(string value)
        {
            Value = new DynValue(value);
        }
    }
}