using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(MemorySetNode memorySetNode)
        {
            void setMemoryValue(int addr, int value, MemoryGetNode.OperandSize size)
            {
                if (size == MemoryGetNode.OperandSize.Byte)
                {
                    if (value > byte.MaxValue)
                        throw new RuntimeException("Operand size mismatch, expected at most a byte.",
                            memorySetNode.Line);

                    Memory.Poke(addr, (byte)value);
                }
                else if (size == MemoryGetNode.OperandSize.Word)
                {
                    if (value > ushort.MaxValue)
                        throw new RuntimeException("Operand size mismatch, expected at most a word.",
                            memorySetNode.Line);

                    Memory.Poke(addr, (short)value);
                }
                else if (size == MemoryGetNode.OperandSize.Dword)
                {
                    Memory.Poke(addr, value);
                }
            }

            if (Memory == null)
                throw new RuntimeException("There is no memory to index.", memorySetNode.Line);

            var address = Visit(memorySetNode.MemoryCellNode.Index).Number;

            if (address % 1 != 0)
                throw new RuntimeException("Memory indexing using fractions is not supported.", memorySetNode.Line);

            if (address >= Memory.Array.Length || address < 0)
                throw new RuntimeException("Memory indexer is out of bounds.", memorySetNode.Line);

            if (memorySetNode.OperationType == MemorySetNode.MemorySetOperation.Expression)
            {
                var value = Visit(memorySetNode.Expression);

                setMemoryValue((int)address, (int)value.Number, memorySetNode.MemoryCellNode.Size);

                return new DynValue(value.Number);
            }
            else if (memorySetNode.OperationType == MemorySetNode.MemorySetOperation.PostIncrement)
            {
                int value;

                switch (memorySetNode.MemoryCellNode.Size)
                {
                    case MemoryGetNode.OperandSize.Byte:
                        value = Memory.Peek8((int)address);

                        if (value + 1 > byte.MaxValue)
                            Memory.Poke((int)address, byte.MinValue);
                        else
                            Memory.Poke((int)address, (byte)(value + 1));

                        break;

                    case MemoryGetNode.OperandSize.Word:
                        value = Memory.Peek16((int)address);

                        if (value + 1 > short.MaxValue)
                            Memory.Poke((int)address, short.MinValue);
                        else
                            Memory.Poke((int)address, (short)(value + 1));

                        break;

                    case MemoryGetNode.OperandSize.Dword:
                        value = Memory.Peek32((int)address);
                        Memory.Poke((int)address, value + 1);

                        break;

                    default: throw new RuntimeException("Invalid operand size.", memorySetNode.Line);
                }

                return new DynValue(value);
            }
            else if (memorySetNode.OperationType == MemorySetNode.MemorySetOperation.PostDecrement)
            {
                int value;

                switch (memorySetNode.MemoryCellNode.Size)
                {
                    case MemoryGetNode.OperandSize.Byte:
                        value = Memory.Peek8((int)address);

                        if (value - 1 < byte.MinValue)
                            Memory.Poke((int)address, byte.MaxValue);
                        else
                            Memory.Poke((int)address, (byte)(value - 1));

                        break;

                    case MemoryGetNode.OperandSize.Word:
                        value = Memory.Peek16((int)address);

                        if (value - 1 > short.MinValue)
                            Memory.Poke((int)address, short.MaxValue);
                        else
                            Memory.Poke((int)address, (short)(value - 1));

                        break;

                    case MemoryGetNode.OperandSize.Dword:
                        value = Memory.Peek32((int)address);

                        if (value - 1 > int.MinValue)
                            Memory.Poke((int)address, int.MaxValue);
                        else
                            Memory.Poke((int)address, value - 1);

                        break;

                    default: throw new RuntimeException("Invalid operand size.", memorySetNode.Line);
                }

                return new DynValue(value);
            }
            else
                throw new RuntimeException("Internal error: unexpected memory set operation type.", memorySetNode.Line);
        }
    }
}