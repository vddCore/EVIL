namespace EVIL.VM.Debugger;

using System;
using System.Collections.Generic;
using System.Threading;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.TranslationEngine;

public sealed class DebuggerSession : IDebuggerSession
{
    private readonly ManualResetEventSlim _resumeEvent = new(false);

    private StepMode _stepMode = StepMode.None;
    private int _stepStartDepth;
    private int _stepStartLine;
    private Chunk? _stepStartChunk;
    private Fiber? _stepTargetFiber;
    private volatile Fiber? _activeFiber;
    private volatile bool _pauseRequested;
    private volatile bool _isPaused;
    private volatile bool _isAllFibersFrozen = true;
    
    public event Action<Fiber, ScriptStackFrame, StopReason>? OnStopped;
    public event Action<Fiber>? OnResumed;
    public event Action<Fiber, Exception>? OnExceptionThrown;
    
    public event Action<Breakpoint>? OnBreakpointBound
    {
        add => BreakpointManager.OnBreakpointBound += value;
        remove => BreakpointManager.OnBreakpointBound -= value;
    }

    event Action<IBreakpoint>? IDebuggerSession.OnBreakpointBound
    {
        add
        {
            if (value != null)
            {
                BreakpointManager.OnBreakpointBound += value;
            }
        }
        
        remove { }
    }

    public VirtualMachineBase VirtualMachine { get; }
    public BreakpointManager BreakpointManager { get; } = new();

    public Fiber? ActiveFiber
    {
        get => _activeFiber;
        set
        {
            if (_activeFiber == value)
            {
                return;
            }

            _activeFiber = value;
            VirtualMachine.Scheduler.Wake();
        }
    }

    public bool IsPaused => _isPaused;
    public bool IsAllFibersFrozen
    {
        get => _isAllFibersFrozen;
        set
        {
            if (_isAllFibersFrozen == value)
            {
                return;
            }

            _isAllFibersFrozen = value;
            VirtualMachine.Scheduler.Wake();
        }
    }

    public DebuggerSession(VirtualMachineBase vm)
    {
        VirtualMachine = vm;
        VirtualMachine.DebuggerSession = this;
    }

    public int AddBreakpoint(
        string file,
        int line,
        int column = 0,
        string? condition = null,
        string? hitCondition = null)
    {
        var bp = BreakpointManager.AddBreakpoint(
            file, 
            line,
            column,
            condition,
            hitCondition
        );
        
        return bp.Id;
    }

    public int AddAddressBreakpoint(
        Chunk chunk,
        int ip,
        string? condition = null,
        string? hitCondition = null)
    {
        var bp = BreakpointManager.AddAddressBreakpoint(
            chunk,
            ip,
            condition,
            hitCondition
        );

        return bp.Id;
    }

    public bool RemoveBreakpoint(int breakpointId)
        => BreakpointManager.RemoveBreakpoint(breakpointId);

    public IReadOnlyList<Breakpoint> GetBreakpoints()
        => BreakpointManager.GetBreakpoints();

    IReadOnlyList<IBreakpoint> IDebuggerSession.GetBreakpoints()
        => BreakpointManager.GetBreakpoints();

    public void BindBreakpoints(Chunk chunk)
        => BreakpointManager.BindBreakpoints(chunk);

    public void Continue()
    {
        _stepMode = StepMode.None;
        _stepTargetFiber = null;
        
        ResumeExecution();
    }

    public void StepInto()
    {
        var targetFiber = ActiveFiber ?? VirtualMachine.MainFiber;
        var top = targetFiber.CallStack.Count > 0 
            ? targetFiber.CallStack.Peek() as ScriptStackFrame 
            : null;

        _stepTargetFiber = targetFiber;
        _stepStartDepth = targetFiber.CallStack.Count;
        _stepStartLine = top != null ? top.Chunk.DebugDatabase.GetLineForIP((int)top.IP) : -1;
        _stepStartChunk = top?.Chunk;
        _stepMode = StepMode.StepInto;

        ResumeExecution();
    }

    public void StepOver()
    {
        var targetFiber = ActiveFiber ?? VirtualMachine.MainFiber;
        var top = targetFiber.CallStack.Count > 0 
            ? targetFiber.CallStack.Peek() as ScriptStackFrame 
            : null;

        _stepTargetFiber = targetFiber;
        _stepStartDepth = targetFiber.CallStack.Count;
        _stepStartLine = top != null ? top.Chunk.DebugDatabase.GetLineForIP((int)top.IP) : -1;
        _stepStartChunk = top?.Chunk;
        _stepMode = StepMode.StepOver;

        ResumeExecution();
    }

    public void StepOut()
    {
        var targetFiber = ActiveFiber ?? VirtualMachine.MainFiber;

        _stepTargetFiber = targetFiber;
        _stepStartDepth = targetFiber.CallStack.Count;
        _stepStartChunk = null;
        _stepStartLine = -1;
        _stepMode = StepMode.StepOut;

        ResumeExecution();
    }

    public void StepInstruction()
    {
        _stepTargetFiber = ActiveFiber ?? VirtualMachine.MainFiber;
        _stepMode = StepMode.StepInstruction;

        ResumeExecution();
    }

    public void Pause()
    {
        _pauseRequested = true;
    }

    public void Detach()
    {
        _stepMode = StepMode.None;
        BreakpointManager.ClearBreakpoints();
        VirtualMachine.DebuggerSession = null;
        
        ResumeExecution();
    }

    private void ResumeExecution()
    {
        _isPaused = false;
        _resumeEvent.Set();
        
        VirtualMachine.Scheduler.Wake();
    }

    public bool ShouldBreakAt(Fiber fiber, ScriptStackFrame frame, int ip, OpCode opCode, out StopReason reason)
    {
        reason = StopReason.Step;

        if (frame.Chunk.DebugDatabase.DefinedInFile.StartsWith("builtin::", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_pauseRequested)
        {
            _pauseRequested = false;
            reason = StopReason.Pause;
            return true;
        }

        if (BreakpointManager.ShouldBreakAt(frame.Chunk, ip, fiber, frame, this, out _))
        {
            reason = StopReason.Breakpoint;
            return true;
        }

        if (_stepMode == StepMode.None)
        {
            return false;
        }

        if (_stepTargetFiber != null && _stepTargetFiber != fiber)
        {
            return false;
        }

        var currentDepth = fiber.CallStack.Count;
        var currentLine = frame.Chunk.DebugDatabase.GetLineForIP(ip);
        var currentChunk = frame.Chunk;

        switch (_stepMode)
        {
            case StepMode.StepInstruction:
            {
                reason = StopReason.Step;
                return true;
            }

            case StepMode.StepInto:
            {
                if (currentLine > 0 
                    && (currentLine != _stepStartLine 
                        || currentChunk != _stepStartChunk 
                        || currentDepth != _stepStartDepth))
                {
                    reason = StopReason.Step;
                    return true;
                }
                break;
            }

            case StepMode.StepOver:
            {
                if (currentDepth < _stepStartDepth && currentLine > 0)
                {
                    reason = StopReason.Step;
                    return true;
                }

                if (currentDepth == _stepStartDepth)
                {
                    if (currentChunk != _stepStartChunk 
                        || (currentLine > 0 && currentLine != _stepStartLine))
                    {
                        reason = StopReason.Step;
                        return true;
                    }
                }
                break;
            }

            case StepMode.StepOut:
            {
                if (currentDepth < _stepStartDepth && currentLine > 0)
                {
                    reason = StopReason.Step;
                    return true;
                }
                break;
            }
        }

        return false;
    }

    public void OnBreakEncountered(
        Fiber fiber, 
        ScriptStackFrame frame,
        int ip,
        OpCode opCode,
        StopReason reason)
    {
        ActiveFiber = fiber;
        
        _stepMode = StepMode.None;
        _isPaused = true;
        _resumeEvent.Reset();

        OnStopped?.Invoke(fiber, frame, reason);

        _resumeEvent.Wait();

        _isPaused = false;
        OnResumed?.Invoke(fiber);
    }

    public void NotifyException(Fiber fiber, Exception exception)
    {
        OnExceptionThrown?.Invoke(fiber, exception);
    }

    public IReadOnlyList<Fiber> GetFibers()
        => VirtualMachine.Scheduler.Fibers.ToArray();

    public CallStack GetCallStack(Fiber fiber)
        => fiber.CallStack;

    public IReadOnlyDictionary<string, DynamicValue> GetFrameLocals(ScriptStackFrame frame)
    {
        var result = new Dictionary<string, DynamicValue>();
        if (frame.Locals == null)
            return result;

        for (var i = 0; i < frame.Locals.Length; i++)
        {
            var name = frame.Chunk.DebugDatabase.TryGetLocalName(i, out var n) ? n : $"local_{i}";
            result[name] = frame.Locals[i];
        }

        return result;
    }

    public IReadOnlyDictionary<string, DynamicValue> GetFrameArguments(ScriptStackFrame frame)
    {
        var result = new Dictionary<string, DynamicValue>();
        var startIndex = frame.Chunk.IsSelfAware ? 1 : 0;

        if (frame.Chunk.IsSelfAware && frame.Arguments.Length > 0)
        {
            result["self"] = frame.Arguments[0];
        }

        for (var i = startIndex; i < frame.Arguments.Length; i++)
        {
            var name = frame.Chunk.DebugDatabase.TryGetParameterName(i, out var n) ? n : $"arg_{i}";
            result[name] = frame.Arguments[i];
        }

        return result;
    }

    public IReadOnlyList<Upvalue> GetFrameUpvalues(ScriptStackFrame frame)
        => frame.Upvalues ?? [];

    public DynamicValue EvaluateInFrame(ScriptStackFrame frame, string expression)
    {
        var paramNames = new List<string>();
        var paramValues = new List<DynamicValue>();

        if (frame.Chunk.IsSelfAware && frame.Arguments.Length > 0)
        {
            paramNames.Add("self");
            paramValues.Add(frame.Arguments[0]);
        }

        var argStart = frame.Chunk.IsSelfAware ? 1 : 0;
        for (var i = argStart; i < frame.Arguments.Length; i++)
        {
            if (frame.Chunk.DebugDatabase.TryGetParameterName(i, out var pName))
            {
                if (!paramNames.Contains(pName))
                {
                    paramNames.Add(pName);
                    paramValues.Add(frame.Arguments[i]);
                }
            }
        }

        if (frame.Locals != null)
        {
            for (var i = 0; i < frame.Locals.Length; i++)
            {
                if (frame.Chunk.DebugDatabase.TryGetLocalName(i, out var lName))
                {
                    if (!paramNames.Contains(lName))
                    {
                        paramNames.Add(lName);
                        paramValues.Add(frame.Locals[i]);
                    }
                }
            }
        }

        string evalSource;
        if (paramNames.Count > 0)
        {
            var paramList = string.Join(", ", paramNames);
            evalSource = $"ret (fn({paramList}) -> ({expression.TrimEnd(';')}))(*);";
        }
        else
        {
            evalSource = $"ret ({expression.TrimEnd(';')});";
        }

        try
        {
            var compiler = new Compiler();
            var chunk = compiler.Compile(evalSource, "<eval>");

            var tempFiber = VirtualMachine.Scheduler.CreateFiber(immunized: true);
            tempFiber.Schedule(chunk, [..paramValues]);

            while (tempFiber.State == FiberState.Running)
            {
                tempFiber.Step();
            }

            var result = tempFiber.EvaluationStack.Count > 0 
                ? tempFiber.EvaluationStack.Pop()
                : DynamicValue.Nil;

            VirtualMachine.Scheduler.Fibers.Remove(tempFiber.GetHashCode());
            return result;
        }
        catch (Exception ex)
        {
            return new DynamicValue($"<Evaluation error: {ex.Message}>");
        }
    }

    public void SetFrameLocal(ScriptStackFrame frame, string variableName, DynamicValue value)
    {
        if (frame.Locals != null)
        {
            for (var i = 0; i < frame.Locals.Length; i++)
            {
                if (frame.Chunk.DebugDatabase.TryGetLocalName(i, out var name) 
                    && name == variableName)
                {
                    frame.Locals[i] = value;
                    return;
                }
            }
        }

        for (var i = 0; i < frame.Arguments.Length; i++)
        {
            if (frame.Chunk.DebugDatabase.TryGetParameterName(i, out var name) 
                && name == variableName)
            {
                frame.Arguments[i] = value;
                return;
            }
        }

        if (variableName == "self" && frame.Chunk.IsSelfAware && frame.Arguments.Length > 0)
        {
            frame.Arguments[0] = value;
            return;
        }

        VirtualMachine.Global[variableName] = value;
    }
}
