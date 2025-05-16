namespace EVIL.Ceres.ExecutionEngine;

using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Diagnostics;

public sealed class ChunkReader(byte[] memory)
{
    public long IP;
    public readonly long Length = memory.Length;

    public OpCode PeekOpCode()
        => (OpCode)memory[IP];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
        => memory[IP++];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int ReadInt32()
    {
        fixed (byte* ptr = &memory[IP])
        {
            int value = Unsafe.ReadUnaligned<int>(ptr);
            IP += sizeof(int);
            
            return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe long ReadInt64()
    {
        fixed (byte* ptr = &memory[IP])
        {
            long value = Unsafe.ReadUnaligned<long>(ptr);
            IP += sizeof(long);
            
            return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe double ReadDouble()
    {
        fixed (byte* ptr = &memory[IP])
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