namespace EVIL.VirtualMachine.ExecutionEngine;

using System;

using EVIL.VirtualMachine.ExecutionEngine.Collections;
using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Debugging;

public abstract class VirtualMachineBase
{
    public Table Global { get; }
    public FiberScheduler Scheduler { get; }
    public Fiber MainFiber { get; }
    public IDebuggerSession? DebuggerSession { get; set; }
    
    protected VirtualMachineBase(FiberCrashHandler? crashHandler = null)
        : this(new Table(), crashHandler)
    {
    }
        
    protected VirtualMachineBase(Table global, FiberCrashHandler? crashHandler = null)
    {
        Global = global;
        Scheduler = new FiberScheduler(this, crashHandler ?? DefaultCrashHandler);
        MainFiber = Scheduler.CreateFiber(true);
    }

    private static void DefaultCrashHandler(Fiber fiber, Exception exception)
        => throw new VirtualMachineException("A fiber has crashed.", exception);
}