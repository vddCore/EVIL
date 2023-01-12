using EVIL.AST.Base;
using EVIL.AST.Enums;

namespace EVIL.AST.Nodes
{
    public class BinaryOperationNode : AstNode
    {
        public AstNode LeftOperand { get; }
        public BinaryOperationType Type { get; }
        public AstNode RightOperand { get; }

        public BinaryOperationNode(AstNode leftOperand, BinaryOperationType type, AstNode rightOperand)
        {
            LeftOperand = leftOperand;
            Type = type;
            RightOperand = rightOperand;
        }
    }
}