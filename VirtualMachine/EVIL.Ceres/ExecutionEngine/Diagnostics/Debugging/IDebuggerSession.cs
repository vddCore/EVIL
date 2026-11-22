namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

using System;
using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public interface IDebuggerSession
{
    event Action<Fiber, ScriptStackFrame, StopReason>? OnStopped;
    event Action<Fiber>? OnResumed;
    event Action<Fiber, Exception>? OnExceptionThrown;
    event Action<IBreakpoint>? OnBreakpointBound;
    
    VirtualMachineBase VirtualMachine { get; }
    Fiber? ActiveFiber { get; set; }
    bool IsPaused { get; }
    bool IsAllFibersFrozen { get; set; }

    void BindBreakpoints(Chunk chunk);

    int AddBreakpoint(string file, int line, int column = 0, string? condition = null, string? hitCondition = null);
    bool RemoveBreakpoint(int breakpointId);
    IReadOnlyList<IBreakpoint> GetBreakpoints();

    void Continue();
    void StepInto();
    void StepOver();
    void StepOut();
    void StepInstruction();
    void Pause();
    void Detach();

    bool ShouldBreakAt(Fiber fiber, ScriptStackFrame frame, int ip, OpCode opCode, out StopReason reason);
    void OnBreakEncountered(Fiber fiber, ScriptStackFrame frame, int ip, OpCode opCode, StopReason reason);

    IReadOnlyList<Fiber> GetFibers();
    CallStack GetCallStack(Fiber fiber);
    IReadOnlyDictionary<string, DynamicValue> GetFrameLocals(ScriptStackFrame frame);
    IReadOnlyDictionary<string, DynamicValue> GetFrameArguments(ScriptStackFrame frame);
    IReadOnlyList<Upvalue> GetFrameUpvalues(ScriptStackFrame frame);

    DynamicValue EvaluateInFrame(ScriptStackFrame frame, string expression);
    void SetFrameLocal(ScriptStackFrame frame, string variableName, DynamicValue value);
}
