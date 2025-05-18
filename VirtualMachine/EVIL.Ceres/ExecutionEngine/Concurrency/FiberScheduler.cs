namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed class FiberScheduler(
    VirtualMachineBase vm,
    FiberCrashHandler defaultCrashHandler,
    int initialCapacity = 16)
{
    private FiberCrashHandler _defaultCrashHandler = defaultCrashHandler;
    private volatile bool _running;
    
    public bool IsRunning => _running;
    
    public ConcurrentFiberCollection Fibers { get; } = new(initialCapacity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessFiber(Fiber fiber)
    {
        var state = fiber.State;
        
        switch (state)
        {
            case FiberState.Fresh:
                fiber.Resume();
                break;
            
            case FiberState.Paused:
            case FiberState.Crashed:
            case FiberState.Finished when fiber.ImmuneToCollection:
                return;
            
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
        }
    }
    
    public void Run()
    {
        _running = true;
        
        while (_running)
        {
            try
            {
                foreach (var (_, fiber) in Fibers.Entries)
                {
                    ProcessFiber(fiber);
                }
                
                Fibers.RemoveFinished();
            }
            catch
            {
                /***
                 * Suppress exception from bubbling up,
                 * causing a StackOverflowException
                 ***/
            }
        }
    }
    
    public void Stop() 
        => _running = false;
    
    public void SetDefaultCrashHandler(FiberCrashHandler crashHandler) => 
        _defaultCrashHandler = crashHandler;
    
    public Fiber CreateFiber(
        bool immunized,
        FiberCrashHandler? crashHandler = null,
        Dictionary<string, ClosureContext>? closureContexts = null)
    {
        var fiber = new Fiber(
            vm,
            crashHandler ?? _defaultCrashHandler,
            closureContexts
        );
        
        if (immunized)
        {
            fiber.Immunize();
        }
        
        Fibers.Add(fiber);
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
