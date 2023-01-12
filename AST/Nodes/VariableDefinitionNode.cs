using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class VariableDefinitionNode : AstNode
    {
        public string Identifier { get; }
        public AstNode Right { get; }

        public VariableDefinitionNode(string identifier, AstNode right)
        {
            Identifier = identifier;
            Right = right;
        }
    }
}