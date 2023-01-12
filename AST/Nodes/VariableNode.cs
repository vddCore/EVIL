using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class VariableNode : AstNode
    {
        public string Identifier { get; }

        public VariableNode(string identifier)
        {
            Identifier = identifier;
        }
    }
}