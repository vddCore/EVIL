using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class AssignmentNode : AstNode
    {
        public VariableNode Variable { get; }
        public AstNode Right { get; }
        public bool LocalScope { get; }

        public AssignmentNode(VariableNode variable, AstNode right, bool localScope)
        {
            Variable = variable;
            Right = right;
            LocalScope = localScope;
        }
    }
}