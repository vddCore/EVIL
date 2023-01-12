using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class MemoryGetNode : AstNode
    {
        public enum OperandSize
        {
            Byte,
            Word,
            Dword
        }

        public OperandSize Size { get; }
        public AstNode Index { get; }

        public MemoryGetNode(AstNode index) 
            : this(index, OperandSize.Byte) { }

        public MemoryGetNode(AstNode index, OperandSize size)
        {
            Index = index;
            Size = size;
        }
    }
}
