using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class MemorySetNode : AstNode
    {
        public enum MemorySetOperation
        {
            Expression,
            PostIncrement,
            PostDecrement
        }

        public MemorySetOperation OperationType { get; }

        public MemoryGetNode MemoryCellNode { get; }
        public AstNode Expression { get; }

        public MemorySetNode(MemoryGetNode memoryCellNode, MemorySetOperation operation)
        {
            MemoryCellNode = memoryCellNode;
            OperationType = operation;
        }

        public MemorySetNode(MemoryGetNode memoryCellNode, AstNode expression) : 
            this(memoryCellNode, MemorySetOperation.Expression)
        {
            Expression = expression;
        }
    }
}
