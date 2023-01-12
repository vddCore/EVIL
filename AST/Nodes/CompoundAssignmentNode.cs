using EVIL.AST.Base;
using EVIL.AST.Enums;

namespace EVIL.AST.Nodes
{
    public class CompoundAssignmentNode : AstNode
    {
        public VariableNode Variable { get; }
        public AstNode Right { get; }
        public CompoundAssignmentType Operation { get; }

        public CompoundAssignmentNode(VariableNode variable, AstNode right, CompoundAssignmentType operation)
        {
            Variable = variable;
            Right = right;
            Operation = operation;
        }
    }
}
