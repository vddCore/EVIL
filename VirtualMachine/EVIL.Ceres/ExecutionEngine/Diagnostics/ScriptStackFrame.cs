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
    private Stack<ProtectedRegionActivation>? _protectedRegionStack;
        
    private ValueEnumerator[]? _enumeratorStack;
    private int _enumeratorCount;
    private bool _argumentsRented;
    private bool _localsRented;

    public Fiber Fiber { get; private set; }
    public Chunk Chunk { get; private set; }

    public DynamicValue[] Arguments { get; private set; }
    public EvilArray ExtraArguments => _extraArguments ??= GetExtraArgumentsArray();

    public DynamicValue[]? Locals { get; private set; }
    public Upvalue[]? Upvalues { get; private set; }
    public Stack<ProtectedRegionActivation> ProtectedRegionStack => _protectedRegionStack ??= new();

    public CallFrame CallFrame => new(Chunk, Upvalues, (int)PreviousOpCodeIP, (int)IP);
    public long PreviousOpCodeIP { get; private set; }
    public long IP => _chunkReader.IP;

    public bool IsProtectedState => _protectedRegionStack is { Count: > 0 };

    internal ref ValueEnumerator CurrentEnumerator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_enumeratorCount <= 0 || _enumeratorStack == null)
            {
                throw new VirtualMachineException("Attempt to iterate without an active iterator.");
            }

            return ref _enumeratorStack[_enumeratorCount - 1];
        }
    }

    internal ScriptStackFrame(Fiber fiber)
    {
        Fiber = fiber;
        Chunk = null!;
        Arguments = [];
        _chunkReader = new ChunkReader([]);
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue[] args)
        : this(fiber)
    {
        Reset(fiber, chunk, args);
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, ValueStack stack, int argumentCount)
        : this(fiber)
    {
        Reset(fiber, chunk, stack, argumentCount);
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue a)
        : this(fiber)
    {
        Reset(fiber, chunk, a);
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue a, DynamicValue b)
        : this(fiber)
    {
        Reset(fiber, chunk, a, b);
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c)
        : this(fiber)
    {
        Reset(fiber, chunk, a, b, c);
    }

    internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c, DynamicValue d)
        : this(fiber)
    {
        Reset(fiber, chunk, a, b, c, d);
    }

    internal void Reset(Fiber fiber, Chunk chunk)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        if (chunk.ParameterCount == 0)
        {
            Arguments = [];
            _argumentsRented = false;
        }
        else
        {
            var len = chunk.ParameterCount;
            Arguments = fiber.RentArguments(len);
            _argumentsRented = true;

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
        }

        if (chunk.LocalCount > 0)
        {
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void Reset(Fiber fiber, Chunk chunk, DynamicValue a)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        var len = Math.Max(chunk.ParameterCount, 1);
        Arguments = fiber.RentArguments(len);
        _argumentsRented = true;

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
        Arguments[0] = a;

        if (chunk.LocalCount > 0)
        {
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void Reset(Fiber fiber, Chunk chunk, DynamicValue a, DynamicValue b)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        var len = Math.Max(chunk.ParameterCount, 2);
        Arguments = fiber.RentArguments(len);
        _argumentsRented = true;

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
        Arguments[0] = a;
        Arguments[1] = b;

        if (chunk.LocalCount > 0)
        {
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void Reset(Fiber fiber, Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        var len = Math.Max(chunk.ParameterCount, 3);
        Arguments = fiber.RentArguments(len);
        _argumentsRented = true;

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
        Arguments[0] = a;
        Arguments[1] = b;
        Arguments[2] = c;

        if (chunk.LocalCount > 0)
        {
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void Reset(Fiber fiber, Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c, DynamicValue d)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        var len = Math.Max(chunk.ParameterCount, 4);
        Arguments = fiber.RentArguments(len);
        _argumentsRented = true;

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
        Arguments[0] = a;
        Arguments[1] = b;
        Arguments[2] = c;
        Arguments[3] = d;

        if (chunk.LocalCount > 0)
        {
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void Reset(Fiber fiber, Chunk chunk, ValueStack stack, int argumentCount)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        var paramCount = chunk.ParameterCount;
        if (paramCount == 0 && argumentCount == 0)
        {
            Arguments = [];
            _argumentsRented = false;
        }
        else
        {
            var len = Math.Max(paramCount, argumentCount);
            Arguments = fiber.RentArguments(len);
            _argumentsRented = true;

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

            stack.PopTo(Arguments.AsSpan(0, argumentCount));
        }

        if (chunk.LocalCount > 0)
        {
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void Reset(Fiber fiber, Chunk chunk, DynamicValue[] args)
    {
        Fiber = fiber;
        Chunk = chunk ?? throw new InvalidOperationException("Attempt to invoke a null chunk.");
        PreviousOpCodeIP = 0;
        _extraArguments = null;
        _protectedRegionStack?.Clear();
        ClearEnumerators();

        if (chunk.ParameterCount == 0 && args.Length == 0)
        {
            Arguments = [];
            _argumentsRented = false;
        }
        else if (args.Length >= chunk.ParameterCount && chunk.ParameterInitializers.Count == 0)
        {
            Arguments = args;
            _argumentsRented = false;
        }
        else
        {
            var len = Math.Max(chunk.ParameterCount, args.Length);
            Arguments = fiber.RentArguments(len);
            _argumentsRented = true;

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
            Locals = fiber.RentLocals(chunk.LocalCount);
            _localsRented = true;
        }
        else
        {
            Locals = null;
            _localsRented = false;
        }

        Upvalues = chunk.Upvalues;
        _chunkReader.Reset(chunk.Code);
    }

    internal void ReleaseArrays(Fiber fiber)
    {
        if (_openLocalUpvalues != null)
        {
            Array.Clear(_openLocalUpvalues, 0, _openLocalUpvalues.Length);
        }

        if (_openArgumentUpvalues != null)
        {
            Array.Clear(_openArgumentUpvalues, 0, _openArgumentUpvalues.Length);
        }

        if (Arguments.Length > 0 && _argumentsRented)
        {
            fiber.ReturnArguments(Arguments);
            Arguments = [];
            _argumentsRented = false;
        }

        if (Locals != null && _localsRented)
        {
            fiber.ReturnLocals(Locals);
            Locals = null;
            _localsRented = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Upvalue GetOrCreateLocalUpvalue(int localId)
    {
        if (Locals == null || (uint)localId >= (uint)Locals.Length)
        {
            return new Upvalue(DynamicValue.Nil);
        }

        if (_openLocalUpvalues == null || _openLocalUpvalues.Length < Locals.Length)
        {
            _openLocalUpvalues = new Upvalue?[Locals.Length];
        }

        return _openLocalUpvalues[localId] ??= new Upvalue(Locals[localId], Locals, localId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Upvalue GetOrCreateArgumentUpvalue(int argId)
    {
        if ((uint)argId >= (uint)Arguments.Length)
        {
            return new Upvalue(DynamicValue.Nil);
        }

        if (_openArgumentUpvalues == null || _openArgumentUpvalues.Length < Arguments.Length)
        {
            _openArgumentUpvalues = new Upvalue?[Arguments.Length];
        }

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
            var limit = Math.Min(_openLocalUpvalues.Length, Locals?.Length ?? 0);
            for (var i = 0; i < limit; i++)
            {
                var upval = _openLocalUpvalues[i];
                if (upval != null)
                {
                    upval.Close();
                    Fiber.SaveClosedUpvalue(Chunk.ClosureOwnerId, i, true, upval);
                }
            }
        }

        if (_openArgumentUpvalues != null)
        {
            var limit = Math.Min(_openArgumentUpvalues.Length, Arguments.Length);
            for (var i = 0; i < limit; i++)
            {
                var upval = _openArgumentUpvalues[i];
                if (upval != null)
                {
                    upval.Close();
                    Fiber.SaveClosedUpvalue(Chunk.ClosureOwnerId, i, false, upval);
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
                        var upvalue = new Upvalue(Locals[i]);
                        Fiber.SaveClosedUpvalue(Chunk.ClosureOwnerId, i, true, upvalue);
                    }
                }
            }
            for (var i = 0; i < Arguments.Length; i++)
            {
                if (_openArgumentUpvalues == null || i >= _openArgumentUpvalues.Length || _openArgumentUpvalues[i] == null)
                {
                    var upvalue = new Upvalue(Arguments[i]);
                    Fiber.SaveClosedUpvalue(Chunk.ClosureOwnerId, i, false, upvalue);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnterProtectedBlock(int regionId)
    {
        var region = Chunk.ProtectedRegions[regionId];
        ProtectedRegionStack.Push(
            new ProtectedRegionActivation(
                regionId,
                region,
                Fiber.EvaluationStack.Count
            )
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void RetryProtectedBlock(int regionId)
    {
        var region = Chunk.ProtectedRegions[regionId];
        var stack = ProtectedRegionStack;

        if (stack.Count > 0 && stack.Peek().RegionId == regionId)
        {
            var activation = stack.Peek();
            if (activation.Phase != ProtectedRegionPhase.Body)
            {
                throw new VirtualMachineException(
                    "Cannot retry a protected region outside its body."
                );
            }

            Fiber.EvaluationStack.RestoreTo(activation.EvaluationStackDepth);
        }
        else
        {
            stack.Push(
                new ProtectedRegionActivation(regionId, region, Fiber.EvaluationStack.Count)
            );
        }

        JumpAbsolute(region.BodyStartAddress);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ExitProtectedBlock(int regionId, ProtectedRegionPhase expectedPhase)
    {
        var stack = ProtectedRegionStack;
        if (stack.Count == 0)
        {
            throw new VirtualMachineException("Attempt to leave a protected region without an active region.");
        }

        var activation = stack.Peek();
        if (activation.RegionId != regionId || activation.Phase != expectedPhase)
        {
            throw new VirtualMachineException("Protected-region activation is out of balance.");
        }

        stack.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ProtectedRegionActivation BeginProtectedHandler()
    {
        var stack = ProtectedRegionStack;
        if (stack.Count == 0)
        {
            throw new VirtualMachineException("Attempt to handle an exception without an active protected region.");
        }

        var activation = stack.Peek();
        if (activation.Phase != ProtectedRegionPhase.Body)
        {
            throw new VirtualMachineException("Protected-region handler is already active.");
        }

        stack.Pop();
        activation.BeginHandlerTransfer();
        stack.Push(activation);
        return activation;
    }
        
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
        EnsureEnumeratorCapacity();
        _enumeratorStack![_enumeratorCount++] = new ValueEnumerator(table);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushEnumerator(EvilArray array)
    {
        EnsureEnumeratorCapacity();
        _enumeratorStack![_enumeratorCount++] = new ValueEnumerator(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PopEnumerator()
    {
        if (_enumeratorCount > 0)
        {
            _enumeratorCount--;
            _enumeratorStack![_enumeratorCount].Dispose();
            _enumeratorStack![_enumeratorCount] = default(ValueEnumerator);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureEnumeratorCapacity()
    {
        if (_enumeratorStack == null)
        {
            _enumeratorStack = new ValueEnumerator[4];
        }
        else if (_enumeratorCount >= _enumeratorStack.Length)
        {
            Array.Resize(ref _enumeratorStack, _enumeratorStack.Length * 2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearEnumerators()
    {
        while (_enumeratorCount > 0)
        {
            _enumeratorCount--;
            _enumeratorStack![_enumeratorCount] = default(ValueEnumerator);
            _enumeratorStack![_enumeratorCount].Dispose();
        }
    }

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
        => $"{Chunk?.Name ?? "<anonymous>"}:({Chunk?.DebugDatabase.GetLineForIP((int)PreviousOpCodeIP) ?? 0}) @ {PreviousOpCodeIP:X8}";
}
