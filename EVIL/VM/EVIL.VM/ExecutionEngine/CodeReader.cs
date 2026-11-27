namespace EVIL.VM.ExecutionEngine;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using EVIL.VM.ExecutionEngine.Diagnostics;

public sealed class CodeReader
{
    private byte[] _memory;
    public long IP;
    public long Length { get; private set; }

    public CodeReader(byte[] memory)
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
    public int ReadInt32()
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(_memory.AsSpan((int)IP, sizeof(int)));
        IP += sizeof(int);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(_memory.AsSpan((int)IP, sizeof(long)));
        IP += sizeof(long);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
        => BitConverter.Int64BitsToDouble(ReadInt64());

    public void JumpRelative(long offset)
        => IP += offset;

    public void JumpAbsolute(long address)
        => IP = address;
}