namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using Collections_Array = EVIL.Ceres.ExecutionEngine.Collections.Array;
using Array = System.Array;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public sealed record ScriptStackFrame : StackFrame
{
    private readonly BinaryReader _chunkReader;
    private Collections_Array? _extraArguments;
        
    private readonly Stack<IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>> _collectionEnumerators = new();

    public Fiber Fiber { get; }
    public Chunk Chunk { get; }

    public DynamicValue[] Arguments { get; }
    public Collections_Array ExtraArguments => _extraArguments ??= GetExtraArgumentsArray();

    public DynamicValue[]? Locals { get; }
    public Stack<BlockProtectionInfo> BlockProtectorStack { get; } = new();

    public long PreviousOpCodeIP { get; private set; }
    public long IP => _chunkReader.BaseStream.Position;

    public bool IsProtectedState => Chunk.Flags.HasFlag(ChunkFlags.HasProtectedBlocks) 
                                    && BlockProtectorStack.Count > 0;

    public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>? CurrentEnumerator
    {
        get
        {
            if (_collectionEnumerators.TryPeek(out var enumerator))
                return enumerator;

            return null;
        }
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue[] args)
    {
        Fiber = fiber;
        Chunk = chunk;

        if (chunk == null)
        {
            throw new InvalidOperationException("Attempt to invoke a null chunk.");
        }

        if (chunk.ParameterCount > args.Length)
        {
            Arguments = new DynamicValue[chunk.ParameterCount];
        }
        else
        {
            Arguments = new DynamicValue[args.Length];
        }

        for (var i = 0; i < Arguments.Length; i++)
        {
            if (Chunk.ParameterInitializers.TryGetValue(i, out var initializer))
            {
                Arguments[i] = initializer;
            }

            if (i < args.Length)
            {
                Arguments[i] = args[i];
            }
        }

        if (chunk.LocalCount > 0)
        {
            Locals = new DynamicValue[chunk.LocalCount];
            Array.Fill(Locals, DynamicValue.Nil);
        }

        _chunkReader = Chunk.SpawnCodeReader();
    }

    internal void EnterProtectedBlock(int blockId)
        => BlockProtectorStack.Push(Chunk.ProtectedBlocks[blockId]);

    internal BlockProtectionInfo ExitProtectedBlock()
        => BlockProtectorStack.Pop();
        
    internal DynamicValue[] GetExtraArguments()
    {
        var size = Arguments.Length - Chunk.ParameterCount;
        if (size < 0) size = 0;
        var ret = new DynamicValue[size];

        for (var i = Chunk.ParameterCount; i < Arguments.Length; i++)
        {
            ret[i - Chunk.ParameterCount] = Arguments[i];
        }

        return ret;
    }

    internal Collections_Array GetExtraArgumentsArray()
    {
        var ret = new Collections_Array(Arguments.Length - Chunk.ParameterCount);

        for (var i = Chunk.ParameterCount; i < Arguments.Length; i++)
        {
            ret[i - Chunk.ParameterCount] = Arguments[i];
        }

        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushEnumerator(Table table)
        => _collectionEnumerators.Push(table.GetEnumerator());
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushEnumerator(Collections_Array array)
        => _collectionEnumerators.Push(array.GetEnumerator());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PopEnumerator()
        => _collectionEnumerators.Pop();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Return()
        => _chunkReader.BaseStream.Seek(0, SeekOrigin.End);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Advance() 
        => _chunkReader.BaseStream.Position++;

    internal OpCode PeekOpCode()
    {            
        var opCode = FetchOpCode();

        _chunkReader.BaseStream.Position--;
        return opCode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal OpCode FetchOpCode()
    {
        PreviousOpCodeIP = IP;
        return (OpCode)_chunkReader.ReadByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal byte FetchByte()
        => _chunkReader.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double FetchDouble()
        => _chunkReader.ReadDouble();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FetchInt32()
        => _chunkReader.ReadInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal long FetchInt64()
        => _chunkReader.ReadInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void JumpAbsolute(long address)
        => _chunkReader.BaseStream.Seek(address, SeekOrigin.Begin);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void JumpRelative(long offset)
        => _chunkReader.BaseStream.Seek(offset, SeekOrigin.Current);

    public override void Dispose()
    {
        _chunkReader.Dispose();
    }

    public override string ToString()
        => $"{Chunk.Name}:({Chunk.DebugDatabase.GetLineForIP((int)PreviousOpCodeIP)}) @ {PreviousOpCodeIP:X8}";
}