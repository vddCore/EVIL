namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed class FiberScheduler(
    VirtualMachineBase vm,
    FiberCrashHandler defaultCrashHandler,
    int initialCapacity = 16)
{
    private FiberCrashHandler _defaultCrashHandler = defaultCrashHandler;
    private volatile bool _running;
    private int _idlePassCount;
    private readonly AutoResetEvent _workAvailable = new(false);
    
    internal int IdlePassCount => Volatile.Read(ref _idlePassCount);
    
    public bool IsRunning => _running;
    public ConcurrentFiberCollection Fibers { get; } = new(initialCapacity);
    public FiberCrashHandler DefaultCrashHandler => _defaultCrashHandler;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ProcessFiber(Fiber fiber)
    {
        var debugger = vm.DebuggerSession;
        if (debugger is { IsPaused: true, IsAllFibersFrozen: true } && fiber != debugger.ActiveFiber)
        {
            return false;
        }

        var state = fiber.State;
        
        switch (state)
        {
            case FiberState.Fresh:
            case FiberState.Paused:
            case FiberState.Crashed:
            case FiberState.Finished:
                return false;
            
            case FiberState.Awaiting:
                fiber.CheckAwaiteesCrashing();
                fiber.RemoveFinishedAwaitees();

                if (fiber.State != FiberState.Crashed)
                {
                    fiber.Resume();
                }
                break;
        }

        if (fiber.State == FiberState.Running)
        {
            fiber.Step();
            return fiber.State == FiberState.Running;
        }

        return false;
    }
    
    public void Run()
    {
        _running = true;
        
        while (_running)
        {
            var hasRunnableFiber = false;

            try
            {
                foreach (var (_, fiber) in Fibers.Entries)
                {
                    hasRunnableFiber |= ProcessFiber(fiber);
                }
                
                Fibers.RemoveFinished();
            }
            catch
            {
                /***
                 * Suppress exception from bubbling up,
                 * causing a StackOverflowException
                 ***/
                hasRunnableFiber = true;
            }

            if (!hasRunnableFiber && _running)
            {
                Interlocked.Increment(ref _idlePassCount);
                _workAvailable.WaitOne();
            }
        }
    }
    
    public void Stop()
    {
        _running = false;
        _workAvailable.Set();
    }

    internal void Wake()
        => _workAvailable.Set();
    
    public void SetDefaultCrashHandler(FiberCrashHandler crashHandler) => 
        _defaultCrashHandler = crashHandler;
    
    public Fiber CreateFiber(
        bool immunized,
        FiberCrashHandler? crashHandler = null,
        Dictionary<string, ClosureContext>? closureContexts = null,
        Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>? sharedUpvalues = null)
        => CreateFiberWithOwnerUpvalues(
            immunized,
            crashHandler,
            closureContexts,
            sharedUpvalues,
            null
        );

    internal Fiber CreateFiberWithOwnerUpvalues(
        bool immunized,
        FiberCrashHandler? crashHandler,
        Dictionary<string, ClosureContext>? closureContexts,
        Dictionary<(string FunctionName, int Slot, bool IsLocal), Upvalue>? sharedUpvalues,
        Dictionary<(Guid OwnerId, int Slot, bool IsLocal), Upvalue>? sharedOwnerUpvalues)
    {
        var fiber = new Fiber(
            vm,
            crashHandler ?? DefaultCrashHandler,
            closureContexts,
            sharedUpvalues,
            sharedOwnerUpvalues
        );
        
        if (immunized)
        {
            fiber.Immunize();
        }
        
        Fibers.Add(fiber);
        Wake();
        
        return fiber;
    }
    
    public void RemoveCrashedFibers()
    {
        foreach (var kvp in Fibers.Entries)
        {
            if (kvp.Value.State == FiberState.Crashed)
            {
                Fibers.Remove(kvp.Key);
            }
        }
    }
}
