namespace EVIL.VirtualMachine.ExecutionEngine;

using System.Runtime.CompilerServices;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

public sealed class ChunkReader
{
    private byte[] _memory;
    public long IP;
    public long Length { get; private set; }

    public ChunkReader(byte[] memory)
    {
        _memory = memory;
        Length = memory.Length;
        IP = 0;
    }

    public void Reset(byte[] memory)
    {
        _memory = memory;
        Length = memory.Length;
        IP = 0;
    }

    public OpCode PeekOpCode()
        => (OpCode)_memory[IP];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
        => _memory[IP++];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBoolean()
        => _memory[IP++] != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int ReadInt32()
    {
        fixed (byte* ptr = &_memory[IP])
        {
            int value = Unsafe.ReadUnaligned<int>(ptr);
            IP += sizeof(int);
            
            return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe long ReadInt64()
    {
        fixed (byte* ptr = &_memory[IP])
        {
            long value = Unsafe.ReadUnaligned<long>(ptr);
            IP += sizeof(long);
            
            return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe double ReadDouble()
    {
        fixed (byte* ptr = &_memory[IP])
        {
            double value = Unsafe.ReadUnaligned<double>(ptr);
            IP += sizeof(double);
            
            return value;
        }
    }

    public void JumpRelative(long offset)
        => IP += offset;

    public void JumpAbsolute(long address)
        => IP = address;
}