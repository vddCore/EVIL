namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public sealed class Fiber
{
    private const int ProcessingBatchQuantum = 32;
    
    private static long _nextLockOrderId;

    private readonly Queue<(Chunk Chunk, DynamicValue[] Arguments)> _scheduledChunks;
    private readonly HashSet<Fiber> _waitingFor;
    private readonly HashSet<Fiber> _waitingOn;
    private readonly HashSet<Fiber> _pendingAwaiteeCrashes;
    private readonly long _lockOrderId = Interlocked.Increment(ref _nextLockOrderId);
    
    private TaskCompletionSource<bool>? _completionSource;
    private int _queued;

    private readonly Dictionary<string, ClosureContext> _closureContexts;
    private readonly Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue> _sharedUpvalues;
    private readonly Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue> _closedUpvalues = new();
    private readonly Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue> _sharedOwnerUpvalues;
    private readonly Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue> _closedOwnerUpvalues = new();

    private readonly Stack<ScriptStackFrame> _scriptFramePool = new();
    private readonly Stack<DynamicValue[]>[] _argumentsPool = new Stack<DynamicValue[]>[16];
    private readonly Stack<DynamicValue[]>[] _localsPool = new Stack<DynamicValue[]>[32];

    private readonly ExecutionUnit _executionUnit;
    private FiberCrashHandler? _crashHandler;

    internal volatile FiberState _state;
    internal int RegistryId { get; set; }

    internal ChunkInvokeHandler? OnChunkInvoke { get; private set; }
    internal NativeFunctionInvokeHandler? OnNativeFunctionInvoke { get; private set; }

    public Fiber? Parent { get; internal set; }
    
    public FiberCrashHandler? CrashHandler 
        => _crashHandler;
    
    public IReadOnlySet<Fiber> WaitingFor
    {
        get
        {
            lock (_waitingFor)
            {
                return new HashSet<Fiber>(
                    _waitingFor
                );
            }
        }
    }
        
    public IReadOnlyDictionary<string, ClosureContext> ClosureContexts => _closureContexts;

    public VirtualMachineBase VirtualMachine { get; }
    public IDebuggerSession? DebuggerSession => VirtualMachine.DebuggerSession;

    public FiberState State => _state;

    public bool ImmuneToCollection { get; private set; }

    public ValueStack EvaluationStack { get; }
    public CallStack CallStack { get; }

    internal Fiber(
        VirtualMachineBase virtualMachine,
        Dictionary<string, ClosureContext>? closureContexts = null,
        Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>? sharedUpvalues = null,
        Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue>? sharedOwnerUpvalues = null)
    {
        VirtualMachine = virtualMachine;

        _scheduledChunks = new Queue<(Chunk, DynamicValue[])>();
        _waitingFor = [];
        _waitingOn = [];
        _pendingAwaiteeCrashes = [];

        EvaluationStack = new ValueStack();
        CallStack = new CallStack();
        _closureContexts = closureContexts ?? new Dictionary<string, ClosureContext>();
        _sharedUpvalues = sharedUpvalues ?? new Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>();
        _sharedOwnerUpvalues = sharedOwnerUpvalues ?? new Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue>();
            
        _executionUnit = new ExecutionUnit(
            virtualMachine.Global,
            this,
            EvaluationStack,
            CallStack
        );

        _state = FiberState.Fresh;
    }

    internal Fiber(
        VirtualMachineBase virtualMachine, 
        FiberCrashHandler fiberCrashHandler,
        Dictionary<string, ClosureContext>? closureContexts = null,
        Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>? sharedUpvalues = null,
        Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue>? sharedOwnerUpvalues = null)
        : this(virtualMachine, closureContexts, sharedUpvalues, sharedOwnerUpvalues)
    {
        SetCrashHandler(fiberCrashHandler);
    }
    
    internal void TryQueueForExecution()
    {
        if (_state != FiberState.Running
            || Interlocked.CompareExchange(ref _queued, 1, 0) != 0)
        {
            return;
        }

        VirtualMachine.Scheduler.EnqueueReady(RegistryId, this);
    }

    internal void ClearQueued()
        => Volatile.Write(ref _queued, 0);
    
    private Task GetCompletionTask()
    {
        lock (_waitingFor)
        {
            if (IsTerminal())
            {
                return Task.CompletedTask;
            }

            _completionSource ??= new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            
            return _completionSource.Task;
        }
    }

    public void Schedule(Chunk chunk, params DynamicValue[] args)
        => Schedule(chunk, true, args);

    public void Schedule(Chunk chunk, bool resumeImmediately, params DynamicValue[] args)
    {
        if (VirtualMachine.DebuggerSession != null)
        {
            VirtualMachine.DebuggerSession.BindBreakpoints(chunk);
            VirtualMachine.DebuggerSession.ActiveFiber ??= this;
        }

        lock (_scheduledChunks)
        {
            _scheduledChunks.Enqueue((chunk, args));
        }

        if (resumeImmediately)
        {
            Resume();
        }
        else
        {
            VirtualMachine.Scheduler.Wake();
        }
    }

    public void BlockUntilFinished()
    {
        GetCompletionTask().GetAwaiter().GetResult();
    }

    public Task BlockUntilFinishedAsync()
        => GetCompletionTask();

    public string StackTrace(bool skipNativeFrames)
    {
        return StackTrace(
            CallStack.ToArray(skipNativeFrames)
        );
    }
        
    public static string StackTrace(StackFrame[] callStack)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < callStack.Length; i++)
        {
            if (callStack[i] is ScriptStackFrame ssf)
            {
                sb.Append($"at {ssf.Chunk.Name ?? "<unknown>"}");

                if (!string.IsNullOrEmpty(ssf.Chunk.DebugDatabase.DefinedInFile))
                {
                    sb.Append($" in {ssf.Chunk.DebugDatabase.DefinedInFile}");
                }
                    
                sb.Append($": line {ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP)} (IP: {ssf.PreviousOpCodeIP:X8})");
                sb.AppendLine();
            }
            else if (callStack[i] is NativeStackFrame nsf)
            {
                sb.AppendLine($"at clr!{nsf.NativeFunction.Method.DeclaringType!.FullName}::{nsf.NativeFunction.Method.Name}");
            }
        }

        return sb.ToString();
    }

    public void Step()
    {
        try
        {
            if (_state != FiberState.Running)
            {
                return;
            }

            if (CallStack.Count == 0)
            {
                lock (CallStack)
                {
                    if (CallStack.Count == 0)
                    {
                        lock (_scheduledChunks)
                        {
                            if (_scheduledChunks.TryDequeue(out var info))
                            {
                                Invoke(info.Chunk, info.Arguments);
                            }
                            else
                            {
                                EnterFinishedState();
                                return;
                            }
                        }
                    }
                }
            }

            _executionUnit.Step(ProcessingBatchQuantum);

            if (CallStack.Count == 0)
            {
                lock (CallStack)
                {
                    if (CallStack.Count == 0)
                    {
                        lock (_scheduledChunks)
                        {
                            if (_scheduledChunks.TryDequeue(out var info))
                            {
                                Invoke(info.Chunk, info.Arguments);
                            }
                            else
                            {
                                EnterFinishedState();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (e is RecoverableVirtualMachineException rvme)
            {
                try
                {
                    ThrowFromNative(
                        new Error(
                            new Table { { "native_exception", DynamicValue.FromObject(rvme) } },
                            rvme.Message
                        )
                    );
                }
                catch (Exception e2)
                {
                    EnterCrashState();

                    if (_crashHandler != null)
                    {
                        _crashHandler(this, e2);
                    }
                }
            }
            else
            {
                EnterCrashState();

                if (_crashHandler != null)
                {
                    _crashHandler(this, e);
                }
            }
        }
    }

    public void Yield()
    {
        if (_state != FiberState.Running)
        {
            return;
        }

        _state = FiberState.Paused;
    }

    public void Yield(Fiber to)
    {
        if (_state != FiberState.Running)
        {
            return;
        }

        _state = FiberState.Paused;
        to.Resume();
    }

    public void Await()
    {
        var hasPendingCrash = false;
        lock (_waitingFor)
        {
            if (_waitingFor.Count == 0 && _pendingAwaiteeCrashes.Count == 0)
            {
                return;
            }

            _state = FiberState.Awaiting;
            hasPendingCrash = _pendingAwaiteeCrashes.Count != 0;
        }

        if (hasPendingCrash)
        {
            VirtualMachine.Scheduler.EnqueuePendingAwaiteeCrash(this);
        }
        else
        {
            VirtualMachine.Scheduler.Wake();
        }
    }

    public void WaitFor(Fiber fiber)
    {
        if (fiber == this)
        {
            throw new FiberException("A fiber cannot wait for self.");
        }

        var shouldWake = true;
        var pendingCrash = false;
        WithOrderedLocks([this, fiber], () =>
        {
            if (fiber._state == FiberState.Finished)
            {
                shouldWake = false;
                return;
            }

            if (fiber._state == FiberState.Crashed)
            {
                _pendingAwaiteeCrashes.Add(fiber);
                pendingCrash = true;
            }
            else if (_waitingFor.Add(fiber))
            {
                fiber._waitingOn.Add(this);
            }

            _state = FiberState.Awaiting;
        });

        if (shouldWake)
        {
            if (pendingCrash)
            {
                VirtualMachine.Scheduler.EnqueuePendingAwaiteeCrash(this);
            }
            else
            {
                VirtualMachine.Scheduler.Wake();
            }
        }
    }

    public void StopWaitingFor(Fiber fiber)
    {
        var resumed = false;
        WithOrderedLocks([this, fiber], () =>
        {
            _waitingFor.Remove(fiber);
            fiber._waitingOn.Remove(this);
            _pendingAwaiteeCrashes.Remove(fiber);

            if (_state == FiberState.Awaiting
                && _waitingFor.Count == 0
                && _pendingAwaiteeCrashes.Count == 0)
            {
                _state = FiberState.Running;
                resumed = true;
            }
        });

        if (resumed)
        {
            TryQueueForExecution();
        }
    }

    public void WaitForAll(params Fiber[] fibers)
    {
        var requestedFibers = new Fiber[fibers.Length];
        System.Array.Copy(fibers, requestedFibers, fibers.Length);

        for (var i = 0; i < requestedFibers.Length; i++)
        {
            if (requestedFibers[i] == this)
            {
                throw new FiberException("A fiber cannot wait for self.");
            }
        }

        var lockFibers = new Fiber[requestedFibers.Length + 1];
        System.Array.Copy(requestedFibers, lockFibers, requestedFibers.Length);
        lockFibers[^1] = this;

        var pendingCrash = false;
        WithOrderedLocks(lockFibers, () =>
        {
            for (var i = 0; i < requestedFibers.Length; i++)
            {
                var fiber = requestedFibers[i];

                if (fiber._state == FiberState.Finished)
                {
                    continue;
                }

                if (fiber._state == FiberState.Crashed)
                {
                    _pendingAwaiteeCrashes.Add(fiber);
                    pendingCrash = true;
                }
                else if (_waitingFor.Add(fiber))
                {
                    fiber._waitingOn.Add(this);
                }
            }

            if (_waitingFor.Count != 0 || _pendingAwaiteeCrashes.Count != 0)
            {
                _state = FiberState.Awaiting;
            }
        });

        if (pendingCrash)
        {
            VirtualMachine.Scheduler.EnqueuePendingAwaiteeCrash(this);
        }
        else
        {
            VirtualMachine.Scheduler.Wake();
        }
    }
        
    public void Resume()
    {
        lock (_waitingFor)
        {
            if (_state == FiberState.Awaiting
                && (_waitingFor.Count != 0 || _pendingAwaiteeCrashes.Count != 0))
            {
                return;
            }

            if (IsTerminal())
            {
                _completionSource = null;
            }

            _state = FiberState.Running;
        }

        TryQueueForExecution();
    }

    public void Stop()
    {
        lock (_scheduledChunks)
        {
            _scheduledChunks.Clear();
        }

        lock (CallStack)
        {
            while (CallStack.Count > 0)
            {
                var f = CallStack.Pop();
                if (f is ScriptStackFrame ssf)
                {
                    ReturnScriptFrame(ssf);
                }
            }
        }
    }
        
    public void Reset()
    {
        Stop();

        lock (EvaluationStack)
        {
            EvaluationStack.Clear();
        }

        while (true)
        {
            Fiber[] waitingFor;
            lock (_waitingFor)
            {
                waitingFor = new Fiber[_waitingFor.Count];
                _waitingFor.CopyTo(waitingFor);
            }

            var lockFibers = new Fiber[waitingFor.Length + 1];
            System.Array.Copy(waitingFor, lockFibers, waitingFor.Length);
            lockFibers[^1] = this;

            var reset = false;
            WithOrderedLocks(lockFibers, () =>
            {
                if (!_waitingFor.SetEquals(waitingFor))
                {
                    return;
                }

                foreach (var fiber in waitingFor)
                {
                    fiber._waitingOn.Remove(this);
                }

                _waitingFor.Clear();
                _pendingAwaiteeCrashes.Clear();

                if (IsTerminal())
                {
                    _completionSource = null;
                }

                _state = FiberState.Fresh;
                reset = true;
            });

            if (reset)
            {
                break;
            }
        }
    }

    public void Immunize()
    {
        ImmuneToCollection = true;
    }

    public void DeImmunize()
    {
        ImmuneToCollection = false;
    }

    public void PushValue(DynamicValue value)
    {
        lock (EvaluationStack)
        {
            EvaluationStack.Push(value);
        }
    }

    public DynamicValue PeekValue()
    {
        lock (EvaluationStack)
        {
            return EvaluationStack.Peek();
        }
    }

    public bool TryPeekValue(out DynamicValue value)
    {
        lock (EvaluationStack)
        {
            return EvaluationStack.TryPeek(out value);
        }
    }

    public DynamicValue PopValue()
    {
        lock (EvaluationStack)
        {
            return EvaluationStack.Pop();
        }
    }

    public bool TryPopValue(out DynamicValue value)
    {
        lock (EvaluationStack)
        {
            return EvaluationStack.TryPop(out value);
        }
    }

    public ClosureContext SetClosureContext(string chunkName)
    {
        if (!_closureContexts.TryGetValue(chunkName, out var value))
        {
            value = _closureContexts[chunkName] = new ClosureContext(chunkName);
        }

        return value;
    }

    public bool TryGetSharedUpvalue(string funcName, int slot, bool isLocal, out Upvalue upvalue)
        => _sharedUpvalues.TryGetValue((funcName, slot, isLocal), out upvalue!);

    public void RegisterSharedUpvalue(string funcName, int slot, bool isLocal, Upvalue upvalue)
        => _sharedUpvalues[(funcName, slot, isLocal)] = upvalue;

    public bool TryGetClosedUpvalue(string funcName, int slot, bool isLocal, out Upvalue upvalue)
        => _closedUpvalues.TryGetValue((funcName, slot, isLocal), out upvalue!);

    public void SaveClosedUpvalue(string funcName, int slot, bool isLocal, Upvalue upvalue)
        => _closedUpvalues[(funcName, slot, isLocal)] = upvalue;

    internal bool TryGetSharedUpvalue(Guid ownerId, int slot, bool isLocal, out Upvalue upvalue)
        => _sharedOwnerUpvalues.TryGetValue((ownerId, slot, isLocal), out upvalue!);

    internal void RegisterSharedUpvalue(Guid ownerId, int slot, bool isLocal, Upvalue upvalue)
        => _sharedOwnerUpvalues[(ownerId, slot, isLocal)] = upvalue;

    internal bool TryGetClosedUpvalue(Guid ownerId, int slot, bool isLocal, out Upvalue upvalue)
        => _closedOwnerUpvalues.TryGetValue((ownerId, slot, isLocal), out upvalue!);

    internal void SaveClosedUpvalue(Guid ownerId, int slot, bool isLocal, Upvalue upvalue)
        => _closedOwnerUpvalues[(ownerId, slot, isLocal)] = upvalue;

    public void SetCrashHandler(FiberCrashHandler? crashHandler)
    {
        _crashHandler = crashHandler;
    }

    public void SetOnChunkInvoke(ChunkInvokeHandler? handler)
    {
        OnChunkInvoke = handler;
    }

    public void SetOnNativeFunctionInvoke(NativeFunctionInvokeHandler? handler)
    {
        OnNativeFunctionInvoke = handler;
    }

    public Fiber SpawnChild(
        bool immunized,
        FiberCrashHandler? crashHandler = null,
        Dictionary<string, ClosureContext>? closureContexts = null)
    {
        var fiber = VirtualMachine.Scheduler.CreateFiberWithOwnerUpvalues(
            immunized,
            crashHandler,
            closureContexts ?? _closureContexts,
            _sharedUpvalues,
            _sharedOwnerUpvalues
        );

        fiber.Parent = this;
        return fiber;
    }

    public DynamicValue ThrowFromNative(DynamicValue value)
    {
        lock (CallStack)
        lock (EvaluationStack)
        {              
            var frame = CallStack.Peek();
            if (frame is NativeStackFrame nsf)
            {
                nsf.HasThrown = true;
                CallStack.Pop();
            }
                
            EvaluationStack.Push(value);
                
            UnwindTryHandle(CallStack.ToArray());
        }
            
        return DynamicValue.Nil;
    }
        
    internal void UnwindTryHandle(StackFrame[] callStackCopy)
    {
        lock (CallStack)
        {
            var exceptionObject = PeekValue();
            var scriptFrame = CallStack.Peek().As<ScriptStackFrame>();
                
            while (CallStack.Count > 1 && !scriptFrame.IsProtectedState)
            {
                scriptFrame.CloseUpvalues();
                CallStack.Pop();
                ReturnScriptFrame(scriptFrame);
                scriptFrame = CallStack.Peek().As<ScriptStackFrame>();
            }

            if (scriptFrame.IsProtectedState)
            {
                var activation = scriptFrame.BeginProtectedHandler();
                EvaluationStack.RestoreTo(activation.EvaluationStackDepth);
                EvaluationStack.Push(exceptionObject);
                scriptFrame.JumpAbsolute(activation.Region.HandlerAddress);
            }
            else
            {
                throw new UserUnhandledExceptionException(
                    "A user-unhandled exception has been thrown.",
                    exceptionObject,
                    callStackCopy
                );
            }
        }
    }
    
    internal void ProcessPendingAwaiteeCrashes()
    {
        Fiber[] pendingCrashes;
        lock (_waitingFor)
        {
            if (_pendingAwaiteeCrashes.Count == 0)
            {
                return;
            }

            pendingCrashes = new Fiber[_pendingAwaiteeCrashes.Count];
            _pendingAwaiteeCrashes.CopyTo(pendingCrashes);
            _pendingAwaiteeCrashes.Clear();
        }

        foreach (var fiber in pendingCrashes)
        {
            if (IsTerminal())
            {
                break;
            }

            try
            {
                PushValue(fiber.PeekValue());
                UnwindTryHandle(CallStack.ToArray());
            }
            catch (Exception e)
            {
                EnterCrashState();
                _crashHandler?.Invoke(this, e);
            }
        }

        lock (_waitingFor)
        {
            if (_state == FiberState.Awaiting
                && _waitingFor.Count == 0
                && _pendingAwaiteeCrashes.Count == 0)
            {
                _state = FiberState.Running;
            }
        }
    }

    private void EnterCrashState()
        => EnterTerminalState(FiberState.Crashed);

    private void EnterFinishedState()
        => EnterTerminalState(FiberState.Finished);

    private void EnterTerminalState(FiberState terminalState)
    {
        while (true)
        {
            Fiber[] waiters;
            lock (_waitingFor)
            {
                waiters = new Fiber[_waitingOn.Count];
                _waitingOn.CopyTo(waiters);
            }

            var lockFibers = new Fiber[waiters.Length + 1];
            System.Array.Copy(
                waiters,
                lockFibers,
                waiters.Length
            );
            lockFibers[^1] = this;

            var transitioned = false;
            WithOrderedLocks(lockFibers, () =>
            {
                if (_waitingOn.Count != waiters.Length)
                {
                    return;
                }

                foreach (var waiter in waiters)
                {
                    if (!_waitingOn.Contains(waiter))
                    {
                        return;
                    }
                }

                if (!IsTerminal())
                {
                    _state = terminalState;
                    SignalCompletionWaiters();

                    foreach (var waiter in waiters)
                    {
                        if (!waiter._waitingFor.Remove(this))
                        {
                            continue;
                        }

                        if (terminalState == FiberState.Crashed)
                        {
                            waiter._pendingAwaiteeCrashes.Add(this);
                        }

                        if (terminalState == FiberState.Finished
                            && waiter._state == FiberState.Awaiting
                            && waiter._waitingFor.Count == 0
                            && waiter._pendingAwaiteeCrashes.Count == 0)
                        {
                            waiter._state = FiberState.Running;
                        }
                    }

                    _waitingOn.Clear();
                }

                transitioned = true;
            });

            if (!transitioned)
            {
                continue;
            }

            foreach (var waiter in waiters)
            {
                if (terminalState == FiberState.Crashed)
                {
                    waiter.VirtualMachine.Scheduler.EnqueuePendingAwaiteeCrash(waiter);
                }
                else
                {
                    waiter.TryQueueForExecution();
                }
            }

            VirtualMachine.Scheduler.Wake();
            return;
        }
    }

    private static void WithOrderedLocks(Fiber[] fibers, Action action)
    {
        System.Array.Sort(fibers, static (left, right) => left._lockOrderId.CompareTo(right._lockOrderId));

        var uniqueCount = 0;
        for (var i = 0; i < fibers.Length; i++)
        {
            if (uniqueCount != 0 && ReferenceEquals(fibers[i], fibers[uniqueCount - 1]))
            {
                continue;
            }

            fibers[uniqueCount++] = fibers[i];
        }

        var lockCount = 0;
        try
        {
            for (; lockCount < uniqueCount; lockCount++)
            {
                Monitor.Enter(fibers[lockCount]._waitingFor);
            }

            action();
        }
        finally
        {
            for (var i = lockCount - 1; i >= 0; i--)
            {
                Monitor.Exit(fibers[i]._waitingFor);
            }
        }
    }

    private bool IsTerminal()
        => _state is FiberState.Finished or FiberState.Crashed;

    private void SignalCompletionWaiters()
        => _completionSource?.TrySetResult(true);

    private void Invoke(Chunk chunk, DynamicValue[] args)
    {
        CallStack.Push(RentScriptFrame(chunk, args));
    }

    internal DynamicValue[] RentArguments(int len)
    {
        if (len == 0)
        {
            return [];
        }
        
        if (len < _argumentsPool.Length)
        {
            var pool = _argumentsPool[len];
            if (pool is { Count: > 0 })
            {
                return pool.Pop();
            }
        }
        
        return new DynamicValue[len];
    }

    internal void ReturnArguments(DynamicValue[] args)
    {
        var len = args.Length;
        if (len == 0)
        {
            return;
        }
        
        System.Array.Clear(args, 0, len);
        if (len < _argumentsPool.Length)
        {
            var pool = _argumentsPool[len] ??= new Stack<DynamicValue[]>();
            pool.Push(args);
        }
    }

    internal DynamicValue[]? RentLocals(int localCount)
    {
        if (localCount == 0)
        {
            return null;
        }

        if (localCount < _localsPool.Length)
        {
            var pool = _localsPool[localCount];
            if (pool is { Count: > 0 })
            {
                return pool.Pop();
            }
        }
        
        return new DynamicValue[localCount];
    }

    internal void ReturnLocals(DynamicValue[]? locals)
    {
        if (locals == null)
        {
            return;
        }
        
        var len = locals.Length;
        System.Array.Clear(locals, 0, len);
        
        if (len < _localsPool.Length)
        {
            var pool = _localsPool[len] ??= new Stack<DynamicValue[]>();
            pool.Push(locals);
        }
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk, ValueStack stack, int argumentCount)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop() 
            : new ScriptStackFrame(this);
        
        frame.Reset(
            this, 
            chunk, 
            stack,
            argumentCount
        );
        
        return frame;
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk, DynamicValue[] args)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop() 
            : new ScriptStackFrame(this);
        
        frame.Reset(this, chunk, args);
        return frame;
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop() 
            : new ScriptStackFrame(this);
        
        frame.Reset(this, chunk);
        return frame;
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk, DynamicValue a)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop() 
            : new ScriptStackFrame(this);
        
        frame.Reset(this, chunk, a);
        return frame;
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk, DynamicValue a, DynamicValue b)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop() 
            : new ScriptStackFrame(this);
        
        frame.Reset(this, chunk, a, b);
        return frame;
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop()
            : new ScriptStackFrame(this);
        
        frame.Reset(this, chunk, a, b, c);
        return frame;
    }

    internal ScriptStackFrame RentScriptFrame(Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c, DynamicValue d)
    {
        var frame = _scriptFramePool.Count > 0 
            ? _scriptFramePool.Pop() 
            : new ScriptStackFrame(this);
        
        frame.Reset(this, chunk, a, b, c, d);
        return frame;
    }

    internal void ReturnScriptFrame(ScriptStackFrame frame)
    {
        frame.ReleaseArrays(this);
        _scriptFramePool.Push(frame);
    }
}