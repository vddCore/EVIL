namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

using _Array = System.Array;

public sealed class ValueStack(int initialCapacity = 1024)
{
    private DynamicValue[] _items = new DynamicValue[initialCapacity];
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(DynamicValue value)
    {
        if (_count == _items.Length)
        {
            _Array.Resize(ref _items, _items.Length * 2);
        }
        
        _items[_count++] = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue Pop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        return _items[--_count];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out DynamicValue value)
    {
        value = DynamicValue.Nil;

        if (_count == 0)
        {
            return false;
        }

        value = _items[--_count];
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
            
        return _items[_count - 1];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out DynamicValue value)
    {
        value = DynamicValue.Nil;
        
        if (_count == 0)
            return false;
            
        value = _items[_count - 1];
        return true;
    }
    
    public void Clear()
    {
        _Array.Clear(_items, 0, _count);
        _count = 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue[] PopArguments(int count)
    {
        if (count == 0)
        {
            return [];
        }

        if (_count < count)
        {
            throw new InvalidOperationException("Stack underflow");
        }

        var args = new DynamicValue[count];
        _count -= count;
        _Array.Copy(_items, _count, args, 0, count);
        return args;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopTo(Span<DynamicValue> destination)
    {
        var count = destination.Length;
        if (count == 0)
        {
            return;
        }

        if (_count < count)
        {
            throw new InvalidOperationException("Stack underflow");
        }

        _count -= count;
        new ReadOnlySpan<DynamicValue>(_items, _count, count).CopyTo(destination);
    }

    public int Count => _count;
}
