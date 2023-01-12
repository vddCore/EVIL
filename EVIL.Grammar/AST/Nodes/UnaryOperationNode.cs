namespace EVIL.Grammar.AST.Nodes
{
    public class UnaryOperationNode : AstNode
    {
        public AstNode Operand { get; }
        public UnaryOperationType Type { get; }

        public UnaryOperationNode(AstNode operand, UnaryOperationType type)
        {
            Operand = operand;
            Type = type;
        }
    }
}