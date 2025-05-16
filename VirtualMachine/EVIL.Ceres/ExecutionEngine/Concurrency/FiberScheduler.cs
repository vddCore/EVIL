namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed class FiberScheduler(
    CeresVM vm,
    FiberCrashHandler defaultCrashHandler,
    int initialCapacity = 16)
{
    private FiberCrashHandler _defaultCrashHandler = defaultCrashHandler;
    private volatile bool _running;
    
    public bool IsRunning => _running;
    
    public ConcurrentFiberCollection Fibers { get; } = new(initialCapacity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessFiber(Fiber fiber)
    {
        var state = fiber.State;
        
        switch (state)
        {
            case FiberState.Awaiting:
                fiber.RemoveFinishedAwaitees();
                fiber.Resume();
                break;
                
            case FiberState.Fresh:
                fiber.Resume();
                break;
                
            case FiberState.Paused:
                return;
                
            case FiberState.Finished:
            case FiberState.Crashed:
                if (fiber.ImmuneToCollection)
                    return;
                break;
                
            default:
                fiber.Resume();
                if (fiber._state != FiberState.Running)
                    return;
                break;
        }
        
        fiber.Step();
    }
    
    public void Run()
    {
        _running = true;
        
        while (_running)
        {
            foreach (var (id, fiber) in Fibers.Entries)
            {
                ProcessFiber(fiber);
                
                if (fiber.State is FiberState.Finished 
                                or FiberState.Crashed 
                                && !fiber.ImmuneToCollection)
                {
                    Fibers.Remove(id);
                }
            }
        }
    }
    
    public void Stop() => _running = false;
    
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
