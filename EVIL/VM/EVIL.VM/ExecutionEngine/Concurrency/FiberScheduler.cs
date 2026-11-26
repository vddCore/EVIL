namespace EVIL.VirtualMachine.ExecutionEngine.Concurrency;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Debugging;

public sealed class FiberScheduler(
    VirtualMachineBase vm,
    FiberCrashHandler defaultCrashHandler,
    int initialCapacity = 16)
{
    private FiberCrashHandler _defaultCrashHandler = defaultCrashHandler;
    private volatile bool _running;
    private int _idlePassCount;
    private int _wakePending;
    private int _schedulerThreadId;
    private long _nextTurn;
    private IDebuggerSession? _previousDebugger;
    private Fiber? _previousActiveFiber;
    private bool _previousDebuggerFrozen;
    private bool _hasPreviousDebuggerState;
    private readonly AutoResetEvent _workAvailable = new(false);
    private readonly ConcurrentQueue<(int Id, Fiber Fiber)> _readyIngress = new();
    private readonly ConcurrentQueue<(int Id, Fiber Fiber)> _pendingCrashIngress = new();
    private readonly PriorityQueue<ReadyEntry, (long Turn, int Id)> _readyFibers = new();
    private readonly SortedDictionary<int, Fiber> _debuggerDeferredFibers = new();
    private readonly SortedDictionary<int, Fiber> _debuggerDeferredCrashes = new();

    private readonly record struct ReadyEntry(long Turn, int Id, Fiber Fiber);
    
    internal int IdlePassCount => Volatile.Read(ref _idlePassCount);
    
    public bool IsRunning => _running;
    public ConcurrentFiberCollection Fibers { get; } = new(initialCapacity);
    public FiberCrashHandler DefaultCrashHandler => _defaultCrashHandler;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDebuggerFrozen(IDebuggerSession? debugger, Fiber fiber)
        => debugger is { IsPaused: true, IsAllFibersFrozen: true }
           && fiber != debugger.ActiveFiber;

    public void Run()
        => Run(null);

    internal void Run(Action? onStarted)
    {
        _running = true;
        Volatile.Write(ref _schedulerThreadId, Environment.CurrentManagedThreadId);

        try
        {
            onStarted?.Invoke();

            while (_running)
            {
                var debugger = vm.DebuggerSession;
                RestoreDebuggerDeferredFibers(debugger);
                ProcessPendingCrashIngress(debugger);
                DrainReadyIngress();

                if (_readyFibers.Count == 0)
                {
                    if (HasPendingWork())
                    {
                        continue;
                    }

                    if (Interlocked.Exchange(ref _wakePending, 0) != 0)
                    {
                        _workAvailable.WaitOne(0);
                        continue;
                    }

                    if (!_running)
                    {
                        break;
                    }

                    Interlocked.Increment(ref _idlePassCount);
                    _workAvailable.WaitOne();
                    Interlocked.Exchange(ref _wakePending, 0);
                    continue;
                }

                var readyEntry = _readyFibers.Dequeue();
                var fiber = readyEntry.Fiber;
                fiber.ClearQueued();

                if (!Fibers.Contains(readyEntry.Id, fiber))
                {
                    if (Fibers.Contains(fiber.RegistryId, fiber))
                    {
                        fiber.TryQueueForExecution();
                    }

                    continue;
                }

                if (IsDebuggerFrozen(debugger, fiber))
                {
                    if (fiber.State == FiberState.Running)
                    {
                        _debuggerDeferredFibers[readyEntry.Id] = fiber;
                    }
                    else
                    {
                        fiber.TryQueueForExecution();
                    }

                    continue;
                }

                if (fiber.State != FiberState.Running)
                {
                    fiber.TryQueueForExecution();
                    Fibers.RemoveFinished(readyEntry.Id, fiber);
                    continue;
                }

                try
                {
                    fiber.Step();
                }
                catch
                {
                    /***
                     * Suppress exceptions from one fiber so later fibers still get their turn.
                     ***/
                }

                Fibers.RemoveFinished(readyEntry.Id, fiber);
                _nextTurn++;

                if (fiber.State == FiberState.Running)
                {
                    fiber.TryQueueForExecution();
                }
            }
        }
        finally
        {
            _running = false;
            Volatile.Write(ref _schedulerThreadId, 0);
        }
    }
    
    public void Stop()
    {
        _running = false;
        _workAvailable.Set();
    }

    internal void Wake()
    {
        if (Interlocked.Exchange(ref _wakePending, 1) == 0
            && Volatile.Read(ref _schedulerThreadId) != Environment.CurrentManagedThreadId)
        {
            _workAvailable.Set();
        }
    }

    internal void EnqueueReady(int id, Fiber fiber)
    {
        _readyIngress.Enqueue((id, fiber));
        Wake();
    }

    internal void EnqueuePendingAwaiteeCrash(Fiber fiber)
    {
        _pendingCrashIngress.Enqueue((fiber.RegistryId, fiber));
        Wake();
    }

    private void DrainReadyIngress()
    {
        var itemsToDrain = _readyIngress.Count;
        for (var i = 0; i < itemsToDrain && _readyIngress.TryDequeue(out var item); i++)
        {
            if (!Fibers.Contains(item.Id, item.Fiber))
            {
                item.Fiber.ClearQueued();
                if (Fibers.Contains(item.Fiber.RegistryId, item.Fiber))
                {
                    item.Fiber.TryQueueForExecution();
                }

                continue;
            }

            if (item.Fiber.State != FiberState.Running)
            {
                item.Fiber.ClearQueued();
                item.Fiber.TryQueueForExecution();
                Fibers.RemoveFinished(item.Id, item.Fiber);
                continue;
            }

            _debuggerDeferredFibers.Remove(item.Id);
            var readyEntry = new ReadyEntry(_nextTurn, item.Id, item.Fiber);
            _readyFibers.Enqueue(readyEntry, (readyEntry.Turn, readyEntry.Id));
        }
    }

    private void ProcessPendingCrashIngress(IDebuggerSession? debugger)
    {
        while (_pendingCrashIngress.TryDequeue(out var item))
        {
            if (!Fibers.Contains(item.Id, item.Fiber))
            {
                Fibers.RemoveFinished(item.Id, item.Fiber);
                continue;
            }

            if (IsDebuggerFrozen(debugger, item.Fiber))
            {
                _debuggerDeferredCrashes[item.Id] = item.Fiber;
                continue;
            }

            try
            {
                item.Fiber.ProcessPendingAwaiteeCrashes();
            }
            catch
            {
                /***
                 * Suppress exceptions from one fiber so later fibers still get their turn.
                 ***/
            }

            Fibers.RemoveFinished(item.Id, item.Fiber);
            item.Fiber.TryQueueForExecution();
        }
    }

    private void RestoreDebuggerDeferredFibers(IDebuggerSession? debugger)
    {
        var debuggerFrozen = debugger is { IsPaused: true, IsAllFibersFrozen: true };
        var activeFiber = debugger?.ActiveFiber;

        if (_hasPreviousDebuggerState
            && ReferenceEquals(debugger, _previousDebugger)
            && debuggerFrozen == _previousDebuggerFrozen
            && activeFiber == _previousActiveFiber)
        {
            return;
        }

        _hasPreviousDebuggerState = true;
        _previousDebugger = debugger;
        _previousDebuggerFrozen = debuggerFrozen;
        _previousActiveFiber = activeFiber;

        foreach (var fiber in _debuggerDeferredFibers.Values)
        {
            fiber.TryQueueForExecution();
        }

        _debuggerDeferredFibers.Clear();

        foreach (var deferredCrash in _debuggerDeferredCrashes)
        {
            _pendingCrashIngress.Enqueue((deferredCrash.Key, deferredCrash.Value));
        }

        _debuggerDeferredCrashes.Clear();
    }

    private bool HasPendingWork()
        => !_readyIngress.IsEmpty
           || !_pendingCrashIngress.IsEmpty
           || _readyFibers.Count != 0;
    
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
        foreach (var kvp in Fibers.ConcurrentEntries)
        {
            if (kvp.Value.State == FiberState.Crashed)
            {
                Fibers.Remove(kvp.Key);
            }
        }
    }
}
