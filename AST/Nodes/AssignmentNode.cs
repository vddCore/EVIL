using EVIL.AST.Base;
using EVIL.AST.Enums;

namespace EVIL.AST.Nodes
{
    public class AssignmentNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }
        
        public AssignmentOperationType OperationType { get; }

        public AssignmentNode(AstNode left, AstNode right, AssignmentOperationType operationType)
        {
            Left = left;
            Right = right;

            OperationType = operationType;
        }
    }
}