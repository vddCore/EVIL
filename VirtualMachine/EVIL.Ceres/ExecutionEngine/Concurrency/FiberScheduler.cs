namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed class FiberScheduler
{
    private readonly CeresVM _vm;
    private FiberCrashHandler _defaultCrashHandler;
    private readonly ConcurrentFiberCollection _fibers;
    private volatile bool _running;
    
    public bool IsRunning => _running;
    
    public ConcurrentFiberCollection Fibers => _fibers;
    
    public FiberScheduler(
        CeresVM vm,
        FiberCrashHandler defaultCrashHandler,
        int initialCapacity = 16)
    {
        _vm = vm;
        _defaultCrashHandler = defaultCrashHandler;
        _fibers = new ConcurrentFiberCollection(initialCapacity);
    }
    
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
                if (fiber.State != FiberState.Running)
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
            foreach (var kvp in _fibers.Entries)
            {
                var fiber = kvp.Value;
                ProcessFiber(fiber);
                
                if ((fiber.State == FiberState.Finished || fiber.State == FiberState.Crashed) 
                    && !fiber.ImmuneToCollection)
                {
                    _fibers.Remove(kvp.Key);
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
            _vm,
            crashHandler ?? _defaultCrashHandler,
            closureContexts
        );
        
        if (immunized)
        {
            fiber.Immunize();
        }
        
        _fibers.Add(fiber);
        return fiber;
    }
    
    public void RemoveCrashedFibers()
    {
        foreach (var kvp in _fibers.Entries)
        {
            if (kvp.Value.State == FiberState.Crashed)
            {
                _fibers.Remove(kvp.Key);
            }
        }
    }
}
