namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using EvilArray = EVIL.Ceres.ExecutionEngine.Collections.Array;
using Array = System.Array;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public sealed record ScriptStackFrame : StackFrame
{
    private readonly ChunkReader _chunkReader;
    private EvilArray? _extraArguments;
    private Upvalue?[]? _openLocalUpvalues;
    private Upvalue?[]? _openArgumentUpvalues;
    private Stack<BlockProtectionInfo>? _blockProtectorStack;
        
    private Stack<IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>>? _collectionEnumerators;

    public Fiber Fiber { get; }
    public Chunk Chunk { get; }

    public DynamicValue[] Arguments { get; }
    public EvilArray ExtraArguments => _extraArguments ??= GetExtraArgumentsArray();

    public DynamicValue[]? Locals { get; }
    public Upvalue[]? Upvalues { get; }
    public Stack<BlockProtectionInfo> BlockProtectorStack => _blockProtectorStack ??= new();

    public long PreviousOpCodeIP { get; private set; }
    public long IP => _chunkReader.IP;

    public bool IsProtectedState => _blockProtectorStack is { Count: > 0 };

    public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>? CurrentEnumerator
    {
        get
        {
            if (_collectionEnumerators != null 
                && _collectionEnumerators.TryPeek(out var enumerator))
            {
                return enumerator;
            }

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

        if (chunk.ParameterCount == 0 && args.Length == 0)
        {
            Arguments = [];
        }
        else if (args.Length >= chunk.ParameterCount && chunk.ParameterInitializers.Count == 0)
        {
            Arguments = args;
        }
        else
        {
            var len = Math.Max(chunk.ParameterCount, args.Length);
            Arguments = new DynamicValue[len];
            
            if (chunk.ParameterInitializers.Count > 0)
            {
                foreach (var (k, v) in chunk.ParameterInitializers)
                {
                    if (k < len)
                    {
                        Arguments[k] = v;
                    }
                }
            }
            Array.Copy(args, 0, Arguments, 0, args.Length);
        }

        if (chunk.LocalCount > 0)
        {
            Locals = new DynamicValue[chunk.LocalCount];
        }

        Upvalues = chunk.Upvalues;
        _chunkReader = Chunk.SpawnCodeReader();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Upvalue GetOrCreateLocalUpvalue(int localId)
    {
        if (Locals == null || (uint)localId >= (uint)Locals.Length)
        {
            return new Upvalue(DynamicValue.Nil);
        }

        _openLocalUpvalues ??= new Upvalue?[Locals.Length];
        return _openLocalUpvalues[localId] ??= new Upvalue(Locals[localId], Locals, localId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Upvalue GetOrCreateArgumentUpvalue(int argId)
    {
        if ((uint)argId >= (uint)Arguments.Length)
        {
            return new Upvalue(DynamicValue.Nil);
        }

        _openArgumentUpvalues ??= new Upvalue?[Arguments.Length];
        return _openArgumentUpvalues[argId] ??= new Upvalue(Arguments[argId], Arguments, argId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasOpenLocalUpvalue(int localId)
    {
        return _openLocalUpvalues != null && (uint)localId < (uint)_openLocalUpvalues.Length && _openLocalUpvalues[localId] != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UpdateOpenLocalUpvalue(int localId, DynamicValue value)
    {
        if (_openLocalUpvalues != null && (uint)localId < (uint)_openLocalUpvalues.Length)
        {
            var upval = _openLocalUpvalues[localId];
            if (upval != null)
            {
                upval.Value = value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InvalidateLocalUpvalue(int localId)
    {
        if (_openLocalUpvalues != null && (uint)localId < (uint)_openLocalUpvalues.Length)
        {
            var upval = _openLocalUpvalues[localId];
            if (upval != null)
            {
                upval.Close();
                _openLocalUpvalues[localId] = null;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InvalidateArgumentUpvalue(int argId)
    {
        if (_openArgumentUpvalues != null && (uint)argId < (uint)_openArgumentUpvalues.Length)
        {
            var upval = _openArgumentUpvalues[argId];
            if (upval != null)
            {
                upval.Close();
                _openArgumentUpvalues[argId] = null;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CloseUpvalues()
    {
        if (_openLocalUpvalues != null)
        {
            for (var i = 0; i < _openLocalUpvalues.Length; i++)
            {
                var upval = _openLocalUpvalues[i];
                if (upval != null)
                {
                    upval.Close();
                    Fiber.SaveClosedUpvalue(Chunk.Name, i, true, upval);
                }
            }
        }

        if (_openArgumentUpvalues != null)
        {
            for (var i = 0; i < _openArgumentUpvalues.Length; i++)
            {
                var upval = _openArgumentUpvalues[i];
                if (upval != null)
                {
                    upval.Close();
                    Fiber.SaveClosedUpvalue(Chunk.Name, i, false, upval);
                }
            }
        }

        if (Chunk.SubChunkCount > 0)
        {
            if (Locals != null)
            {
                for (var i = 0; i < Locals.Length; i++)
                {
                    if (_openLocalUpvalues == null || i >= _openLocalUpvalues.Length || _openLocalUpvalues[i] == null)
                    {
                        Fiber.SaveClosedUpvalue(Chunk.Name, i, true, new Upvalue(Locals[i]));
                    }
                }
            }
            for (var i = 0; i < Arguments.Length; i++)
            {
                if (_openArgumentUpvalues == null || i >= _openArgumentUpvalues.Length || _openArgumentUpvalues[i] == null)
                {
                    Fiber.SaveClosedUpvalue(Chunk.Name, i, false, new Upvalue(Arguments[i]));
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnterProtectedBlock(int blockId)
    {
        _blockProtectorStack ??= new();
        _blockProtectorStack.Push(Chunk.ProtectedBlocks[blockId]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal BlockProtectionInfo ExitProtectedBlock()
        => _blockProtectorStack!.Pop();
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EvilArray GetExtraArgumentsArray()
    {
        var ret = new EvilArray(Arguments.Length - Chunk.ParameterCount);

        for (var i = Chunk.ParameterCount; i < Arguments.Length; i++)
        {
            ret[i - Chunk.ParameterCount] = Arguments[i];
        }

        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushEnumerator(Table table)
    {
        _collectionEnumerators ??= new();
        _collectionEnumerators.Push(table.GetEnumerator());
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushEnumerator(EvilArray array)
    {
        _collectionEnumerators ??= new();
        _collectionEnumerators.Push(array.GetEnumerator());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PopEnumerator()
        => _collectionEnumerators?.Pop();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal OpCode PeekOpCode()
        => _chunkReader.PeekOpCode();

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
        => _chunkReader.JumpAbsolute(address);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void JumpRelative(long offset)
        => _chunkReader.JumpRelative(offset);

    public override string ToString()
        => $"{Chunk.Name}:({Chunk.DebugDatabase.GetLineForIP((int)PreviousOpCodeIP)}) @ {PreviousOpCodeIP:X8}";
}