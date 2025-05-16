namespace EVIL.Ceres.ExecutionEngine;

using System;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public abstract class VirtualMachineBase
{
    public Table Global { get; }
    public FiberScheduler Scheduler { get; }
    public Fiber MainFiber { get; }
    
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