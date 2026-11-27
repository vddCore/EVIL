namespace EVIL.VM.ExecutionEngine;

using System;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;
using EVIL.VM.ExecutionEngine.Marshaling;

public abstract class VirtualMachineBase
{
    public Table Global { get; }
    public FiberScheduler Scheduler { get; }
    public Fiber MainFiber { get; }
    public IDebuggerSession? DebuggerSession { get; set; }
    public MarshalingRegistry Marshal { get; }
    
    protected VirtualMachineBase(
        FiberCrashHandler? crashHandler = null,
        MarshalingRegistry? marshal = null) : this(new Table(), crashHandler, marshal)
    {
    }
        
    protected VirtualMachineBase(
        Table global,
        FiberCrashHandler? crashHandler = null,
        MarshalingRegistry? marshal = null)
    {
        Global = global;
        Marshal = marshal ?? new MarshalingRegistry();
        Scheduler = new FiberScheduler(this, crashHandler ?? DefaultCrashHandler);
        MainFiber = Scheduler.CreateFiber(true);
    }

    private static void DefaultCrashHandler(Fiber fiber, Exception exception)
        => throw new VirtualMachineException("A fiber has crashed.", exception);
}