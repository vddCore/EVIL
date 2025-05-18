namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public sealed class ConcurrentFiberCollection(int initialCapacity = 16)
{
    private int _nextId = 0;

    private readonly ConcurrentDictionary<int, Fiber> _fibers = new(
        Environment.ProcessorCount, 
        initialCapacity
    );
    
    public int Count => _fibers.Count;
    public IReadOnlyDictionary<int, Fiber> Entries => _fibers;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Add(Fiber fiber)
    {
        var id = Interlocked.Increment(ref _nextId);
        _fibers.TryAdd(id, fiber);
        
        return id;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(int id) 
        => _fibers.TryRemove(id, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveFinished()
    {
        var copy = new Dictionary<int, Fiber>(_fibers);

        foreach (var (id, fiber) in copy)
        {
            if (fiber.State is FiberState.Finished 
                            or FiberState.Crashed 
                            && !fiber.ImmuneToCollection)
            {
                Remove(id);
            }
        }
        
        copy.Clear();
    }
    
    public void Clear() 
        => _fibers.Clear();
    
    public Fiber[] ToArray() 
        => _fibers.Values.ToArray();
}
