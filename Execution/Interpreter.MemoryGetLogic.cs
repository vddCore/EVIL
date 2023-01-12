using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(MemoryGetNode memoryGetNode)
        {
            if (Memory == null)
                throw new RuntimeException("There is no memory to index.", memoryGetNode.Line);

            var address = Visit(memoryGetNode.Index).Number;

            if (address % 1 != 0)
                throw new RuntimeException("Memory indexing using fractions is not supported.", memoryGetNode.Line);

            if (address >= Memory.Array.Length || address < 0)
                throw new RuntimeException("Memory indexer is out of bounds.", memoryGetNode.Line);

            if (memoryGetNode.Size == MemoryGetNode.OperandSize.Byte)
            {
                return new DynValue(Memory.Peek8((int)address));
            }
            else if (memoryGetNode.Size == MemoryGetNode.OperandSize.Word)
            {
                return new DynValue(Memory.Peek16((int)address));
            }
            else if (memoryGetNode.Size == MemoryGetNode.OperandSize.Dword)
            {
                return new DynValue(Memory.Peek32((int)address));
            }
            else throw new RuntimeException("Internal error: Unsupported memory operation size.", memoryGetNode.Line);
        }
    }
}
