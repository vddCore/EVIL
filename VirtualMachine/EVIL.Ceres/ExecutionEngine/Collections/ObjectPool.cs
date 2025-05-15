namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Collections.Concurrent;

public sealed class ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _objects;
    private readonly Func<T> _objectGenerator;
    private readonly int _maxSize;

    public ObjectPool(Func<T> objectGenerator, int maxSize = 50)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _maxSize = maxSize;
        _objects = new ConcurrentBag<T>();
    }

    public T Get() => _objects.TryTake(out T? item) ? item : _objectGenerator();

    public void Return(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_objects.Count < _maxSize)
        {
            _objects.Add(item);
        }
    }
}
