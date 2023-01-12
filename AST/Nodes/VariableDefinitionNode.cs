using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class VariableDefinitionNode : AstNode
    {
        public VariableNode Variable { get; }
        public AstNode Right { get; }

        public VariableDefinitionNode(VariableNode variable, AstNode right)
        {
            Variable = variable;
            Right = right;
        }
    }
}