using EVIL.Abstraction;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class NumberNode : AstNode
    {
        public DynValue Value { get; }

        public NumberNode(decimal number)
        {
            Value = new DynValue(number);
        }
    }
}