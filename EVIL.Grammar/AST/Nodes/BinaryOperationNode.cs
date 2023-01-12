namespace EVIL.Grammar.AST.Nodes
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