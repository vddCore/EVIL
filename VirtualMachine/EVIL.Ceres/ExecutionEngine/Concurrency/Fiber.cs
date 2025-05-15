namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System;
using System.Collections.Generic;
using System.Linq;
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

    private readonly ValueStack _evaluationStack;
    private readonly CallStack _callStack;
    private readonly Dictionary<string, ClosureContext> _closureContexts;

    private readonly ExecutionUnit _executionUnit;
    private FiberCrashHandler? _crashHandler;
    internal FiberState _state;

    internal ChunkInvokeHandler? OnChunkInvoke { get; private set; }
    internal NativeFunctionInvokeHandler? OnNativeFunctionInvoke { get; private set; }

    public FiberCrashHandler? CrashHandler => _crashHandler;
    public IReadOnlySet<Fiber> WaitingFor => _waitingFor;
        
    public IReadOnlyDictionary<string, ClosureContext> ClosureContexts => _closureContexts;

    public CeresVM VirtualMachine { get; }

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

    internal Fiber(CeresVM virtualMachine, Dictionary<string, ClosureContext>? closureContexts = null)
    {
        VirtualMachine = virtualMachine;

        _scheduledChunks = new Queue<(Chunk, DynamicValue[])>();
        _waitingFor = new HashSet<Fiber>();

        _evaluationStack = new ValueStack();
        _callStack = new CallStack();
        _closureContexts = closureContexts ?? new Dictionary<string, ClosureContext>();
            
        _executionUnit = new ExecutionUnit(
            virtualMachine.Global,
            this,
            _evaluationStack,
            _callStack
        );

        _state = FiberState.Fresh;
    }

    internal Fiber(
        CeresVM virtualMachine, 
        FiberCrashHandler fiberCrashHandler,
        Dictionary<string, ClosureContext>? closureContexts = null) : this(virtualMachine, closureContexts)
    {
        SetCrashHandler(fiberCrashHandler);
    }

    public void Schedule(Chunk chunk, params DynamicValue[] args)
        => Schedule(chunk, true, args);

    public void Schedule(Chunk chunk, bool resumeImmediately, params DynamicValue[] args)
    {
        lock (_scheduledChunks)
        {
            _scheduledChunks.Enqueue((chunk, args));
        }

        if (resumeImmediately)
        {
            Resume();
        }
    }

    public void BlockUntilFinished()
    {
        while (_state != FiberState.Finished && 
               _state != FiberState.Crashed)
        {
            Thread.Sleep(1);
        }
    }
        
    public async Task BlockUntilFinishedAsync()
    {
        while (_state != FiberState.Finished &&
               _state != FiberState.Crashed)
        {
            await Task.Delay(1);
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
                return;

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
                            _state = FiberState.Finished;
                            return;
                        }
                    }
                }
            }

            _executionUnit.Step();
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
        if (_waitingFor.Count == 0)
        {
            return;
        }

        _state = FiberState.Awaiting;
    }

    public void WaitFor(Fiber fiber)
    {
        if (fiber == this)
        {
            throw new FiberException("A fiber cannot wait for self.");
        }

        if (fiber._state == FiberState.Finished)
        {
            return;
        }

        _waitingFor.Add(fiber);
        Await();
    }

    public void StopWaitingFor(Fiber fiber)
    {
        _waitingFor.Remove(fiber);

        if (_state == FiberState.Awaiting)
        {
            if (_waitingFor.Count == 0)
            {
                _state = FiberState.Running;
            }
        }
    }

    public void WaitForAll(params Fiber[] fibers)
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

        Await();
    }
        
    public void Resume()
    {
        if (_state == FiberState.Awaiting)
        {
            if (_waitingFor.Any())
            {
                return;
            }
        }

        _state = FiberState.Running;
    }

    public void Stop()
    {
        lock (_scheduledChunks)
        {
            _scheduledChunks.Clear();
        }

        lock (_callStack)
        {
            _callStack.DisposeAllAndClear();
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
        }
            
        _state = FiberState.Fresh;
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

    public DynamicValue PopValue()
    {
        lock (_evaluationStack)
        {
            return _evaluationStack.Pop();
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
                _callStack.Pop();
                scriptFrame = _callStack.Peek().As<ScriptStackFrame>();
            }

            if (scriptFrame.IsProtectedState)
            {
                var info = scriptFrame.BlockProtectorStack.Peek();
                scriptFrame.JumpAbsolute(info.HandlerAddress);
            }
            else
            {
                var exceptionObject = PopValue();

                throw new UserUnhandledExceptionException(
                    "A user-unhandled exception has been thrown.",
                    exceptionObject,
                    callStackCopy
                );
            }
        }
    }

    internal void RemoveFinishedAwaitees()
    {
        _waitingFor.RemoveWhere(x => x._state == FiberState.Finished);
    }

    private void EnterCrashState()
    {
        _state = FiberState.Crashed;
    }

    private void Invoke(Chunk chunk, DynamicValue[] args)
    {
        lock (_callStack)
        {
            _callStack.Push(new ScriptStackFrame(this, chunk, args));
        }
    }
}