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
    private readonly Queue<(Chunk Chunk, DynamicValue[] Arguments)> _scheduledChunks;
    private readonly HashSet<Fiber> _waitingFor;
    private TaskCompletionSource<bool>? _completionSource;

    private readonly ValueStack _evaluationStack;
    private readonly CallStack _callStack;
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

    internal ChunkInvokeHandler? OnChunkInvoke { get; private set; }
    internal NativeFunctionInvokeHandler? OnNativeFunctionInvoke { get; private set; }

    public Fiber? Parent { get; internal set; }
    
    public FiberCrashHandler? CrashHandler => _crashHandler;
    public IReadOnlySet<Fiber> WaitingFor
    {
        get
        {
            lock (_waitingFor)
            {
                return new HashSet<Fiber>(_waitingFor);
            }
        }
    }
        
    public IReadOnlyDictionary<string, ClosureContext> ClosureContexts => _closureContexts;

    public VirtualMachineBase VirtualMachine { get; }
    public IDebuggerSession? DebuggerSession => VirtualMachine.DebuggerSession;

    public FiberState State => _state;

    public bool ImmuneToCollection { get; private set; }

    public ValueStack EvaluationStack
    {
        get
        {
            lock (_evaluationStack)
            {
                return _evaluationStack;
            }
        }
    }

    public CallStack CallStack
    {
        get
        {
            lock (_callStack)
            {
                return _callStack;
            }
        }
    }

    internal Fiber(
        VirtualMachineBase virtualMachine,
        Dictionary<string, ClosureContext>? closureContexts = null,
        Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>? sharedUpvalues = null,
        Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue>? sharedOwnerUpvalues = null)
    {
        VirtualMachine = virtualMachine;

        _scheduledChunks = new Queue<(Chunk, DynamicValue[])>();
        _waitingFor = [];

        _evaluationStack = new ValueStack();
        _callStack = new CallStack();
        _closureContexts = closureContexts ?? new Dictionary<string, ClosureContext>();
        _sharedUpvalues = sharedUpvalues ?? new Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>();
        _sharedOwnerUpvalues = sharedOwnerUpvalues ?? new Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue>();
            
        _executionUnit = new ExecutionUnit(
            virtualMachine.Global,
            this,
            _evaluationStack,
            _callStack
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

    private Task GetCompletionTask()
    {
        lock (_waitingFor)
        {
            if (IsTerminal())
            {
                return Task.CompletedTask;
            }

            _completionSource ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return _completionSource.Task;
        }
    }

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

            if (_callStack.Count == 0)
            {
                lock (_callStack)
                {
                    if (_callStack.Count == 0)
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

            const int batchQuantum = 32;
            _executionUnit.Step(batchQuantum);

            if (_callStack.Count == 0)
            {
                lock (_callStack)
                {
                    if (_callStack.Count == 0)
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
        lock (_waitingFor)
        {
            if (_waitingFor.Count == 0)
            {
                return;
            }

            _state = FiberState.Awaiting;
        }

        VirtualMachine.Scheduler.Wake();
    }

    public void WaitFor(Fiber fiber)
    {
        if (fiber == this)
        {
            throw new FiberException("A fiber cannot wait for self.");
        }

        lock (_waitingFor)
        {
            if (fiber._state == FiberState.Finished)
            {
                return;
            }

            _waitingFor.Add(fiber);
            _state = FiberState.Awaiting;
        }

        VirtualMachine.Scheduler.Wake();
    }

    public void StopWaitingFor(Fiber fiber)
    {
        var resumed = false;
        lock (_waitingFor)
        {
            _waitingFor.Remove(fiber);

            if (_state == FiberState.Awaiting && _waitingFor.Count == 0)
            {
                _state = FiberState.Running;
                resumed = true;
            }
        }

        if (resumed)
        {
            VirtualMachine.Scheduler.Wake();
        }
    }

    public void WaitForAll(params Fiber[] fibers)
    {
        lock (_waitingFor)
        {
            for (var i = 0; i < fibers.Length; i++)
            {
                var fiber = fibers[i];

                if (fiber == this)
                {
                    throw new FiberException("A fiber cannot wait for self.");
                }

                if (fiber._state == FiberState.Finished)
                {
                    continue;
                }

                _waitingFor.Add(fibers[i]);
            }

            if (_waitingFor.Count != 0)
            {
                _state = FiberState.Awaiting;
            }
        }

        VirtualMachine.Scheduler.Wake();
    }
        
    public void Resume()
    {
        lock (_waitingFor)
        {
            if (_state == FiberState.Awaiting && _waitingFor.Count != 0)
            {
                return;
            }

            if (IsTerminal())
            {
                _completionSource = null;
            }

            _state = FiberState.Running;
        }

        VirtualMachine.Scheduler.Wake();
    }

    public void Stop()
    {
        lock (_scheduledChunks)
        {
            _scheduledChunks.Clear();
        }

        lock (_callStack)
        {
            while (_callStack.Count > 0)
            {
                var f = _callStack.Pop();
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
            
        lock (_evaluationStack)
        {
            _evaluationStack.Clear();
        }

        lock (_waitingFor)
        {
            _waitingFor.Clear();

            if (IsTerminal())
            {
                _completionSource = null;
            }

            _state = FiberState.Fresh;
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
        lock (_evaluationStack)
        {
            _evaluationStack.Push(value);
        }
    }

    public DynamicValue PeekValue()
    {
        lock (_evaluationStack)
        {
            return _evaluationStack.Peek();
        }
    }

    public bool TryPeekValue(out DynamicValue value)
    {
        lock (_evaluationStack)
        {
            return _evaluationStack.TryPeek(out value);
        }
    }

    public DynamicValue PopValue()
    {
        lock (_evaluationStack)
        {
            return _evaluationStack.Pop();
        }
    }

    public bool TryPopValue(out DynamicValue value)
    {
        lock (_evaluationStack)
        {
            return _evaluationStack.TryPop(out value);
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
        lock (_callStack)
        lock (_evaluationStack)
        {              
            var frame = _callStack.Peek();
            if (frame is NativeStackFrame nsf)
            {
                nsf.HasThrown = true;
                _callStack.Pop();
            }
                
            _evaluationStack.Push(value);
                
            UnwindTryHandle(_callStack.ToArray());
        }
            
        return DynamicValue.Nil;
    }
        
    internal void UnwindTryHandle(StackFrame[] callStackCopy)
    {
        lock (_callStack)
        {
            var scriptFrame = _callStack.Peek().As<ScriptStackFrame>();
                
            while (_callStack.Count > 1 && !scriptFrame.IsProtectedState)
            {
                scriptFrame.CloseUpvalues();
                _callStack.Pop();
                ReturnScriptFrame(scriptFrame);
                scriptFrame = _callStack.Peek().As<ScriptStackFrame>();
            }

            if (scriptFrame.IsProtectedState)
            {
                var info = scriptFrame.BlockProtectorStack.Peek();
                scriptFrame.JumpAbsolute(info.HandlerAddress);
            }
            else
            {
                var exceptionObject = PeekValue();

                throw new UserUnhandledExceptionException(
                    "A user-unhandled exception has been thrown.",
                    exceptionObject,
                    callStackCopy
                );
            }
        }
    }
    
    internal void CheckAwaiteesCrashing()
    {
        Fiber[] waitingFor;
        lock (_waitingFor)
        {
            waitingFor = [.. _waitingFor];
        }

        foreach (var fiber in waitingFor)
        {
            if (fiber._state == FiberState.Crashed)
            {
                try
                {
                    PushValue(fiber.PeekValue());
                    UnwindTryHandle(_callStack.ToArray());
                }
                catch (Exception e)
                {
                    EnterCrashState();
                    _crashHandler?.Invoke(this, e);
                }
            }
        }
    }

    internal void RemoveFinishedAwaitees()
    {
        lock (_waitingFor)
        {
            _waitingFor.RemoveWhere(
                x => x._state is FiberState.Finished
                              or FiberState.Crashed
            );
        }
    }

    private void EnterCrashState()
    {
        lock (_waitingFor)
        {
            _state = FiberState.Crashed;
            SignalCompletionWaiters();
        }

        VirtualMachine.Scheduler.Wake();
    }

    private void EnterFinishedState()
    {
        lock (_waitingFor)
        {
            _state = FiberState.Finished;
            SignalCompletionWaiters();
        }

        VirtualMachine.Scheduler.Wake();
    }

    private bool IsTerminal()
        => _state is FiberState.Finished or FiberState.Crashed;

    private void SignalCompletionWaiters()
        => _completionSource?.TrySetResult(true);

    private void Invoke(Chunk chunk, DynamicValue[] args)
    {
        lock (_callStack)
        {
            _callStack.Push(RentScriptFrame(chunk, args));
        }
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