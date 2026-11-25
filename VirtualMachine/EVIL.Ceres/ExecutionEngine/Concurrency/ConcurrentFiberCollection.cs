namespace EVIL.Ceres.ExecutionEngine.Concurrency;

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public sealed class ConcurrentFiberCollection(int initialCapacity = 16)
{
    private int _nextId = 0;

    internal ConcurrentDictionary<int, Fiber> ConcurrentEntries { get; } = new(
        Environment.ProcessorCount, 
        initialCapacity
    );
    
    public int Count => ConcurrentEntries.Count;
    public IReadOnlyDictionary<int, Fiber> Entries => ConcurrentEntries;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Add(Fiber fiber)
    {
        var id = Interlocked.Increment(ref _nextId);
        fiber.RegistryId = id;
        ConcurrentEntries.TryAdd(id, fiber);
        
        return id;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(int id) 
        => ConcurrentEntries.TryRemove(id, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Contains(int id, Fiber fiber)
        => ConcurrentEntries.TryGetValue(id, out var registeredFiber)
           && ReferenceEquals(registeredFiber, fiber);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void RemoveFinished(int id, Fiber fiber)
    {
        if ((fiber.State is FiberState.Finished or FiberState.Crashed) 
             && !fiber.ImmuneToCollection)
        {
            Remove(id);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveFinished()
    {
        var toRemove = ArrayPool<int>.Shared.Rent(
            ConcurrentEntries.Count
        );
        
        var removeCount = 0;
        
        foreach (var (id, fiber) in ConcurrentEntries)
        {
            if (fiber.State is FiberState.Finished 
                            or FiberState.Crashed 
                            && !fiber.ImmuneToCollection)
            {
                toRemove[removeCount++] = id;
            }
        }
        
        for (var i = 0; i < removeCount; i++)
        {
            Remove(toRemove[i]);
        }
        
        ArrayPool<int>.Shared.Return(
            toRemove,
            clearArray: true
        );
    }
    
    public void Clear() 
        => ConcurrentEntries.Clear();
    
    public Fiber[] ToArray() => [..ConcurrentEntries.OrderBy(entry => entry.Key)
                                                    .Select(entry => entry.Value)];
}
