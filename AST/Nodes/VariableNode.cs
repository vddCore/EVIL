using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class VariableNode : AstNode
    {
        public string Name { get; }

        public VariableNode(string name)
        {
            Name = name;
        }
    }
}