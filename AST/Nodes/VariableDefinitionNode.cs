using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class VariableDefinitionNode : AstNode
    {
        public VariableNode Variable { get; }
        public AssignmentNode Assignment { get; }

        public VariableDefinitionNode(VariableNode variable, AssignmentNode assignment)
        {
            Variable = variable;
            Assignment = assignment;
        }
    }
}