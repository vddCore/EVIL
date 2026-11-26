namespace EVIL.VirtualMachine.ExecutionEngine.Collections;

using System;
using System.Runtime.CompilerServices;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

using _Array = System.Array;

public sealed class ValueStack(int initialCapacity = 1024)
{
    private readonly int _initialCapacity = initialCapacity;
    private DynamicValue[] _items = new DynamicValue[initialCapacity];

    public int Count { get; private set; }
    internal int Capacity => _items.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(DynamicValue value)
    {
        if (Count == _items.Length)
        {
            _Array.Resize(ref _items, _items.Length * 2);
        }
        
        _items[Count++] = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        return _items[--Count];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out DynamicValue value)
    {
        value = DynamicValue.Nil;

        if (Count == 0)
        {
            return false;
        }

        value = _items[--Count];
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue Peek()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
            
        return _items[Count - 1];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out DynamicValue value)
    {
        value = DynamicValue.Nil;
        
        if (Count == 0)
        {
            return false;
        }
            
        value = _items[Count - 1];
        return true;
    }
    
    public void Clear()
    {
        _Array.Clear(_items, 0, Count);
        Count = 0;
    }

    internal void TrimExcess()
    {
        var targetCapacity = Math.Max(_initialCapacity, Count);
        if (targetCapacity < _items.Length)
        {
            _Array.Resize(ref _items, targetCapacity);
        }
    }

    internal void RestoreTo(int count)
    {
        if (count < 0 || count > Count)
        {
            throw new InvalidOperationException(
                "Invalid evaluation stack checkpoint."
            );
        }

        _Array.Clear(
            _items,
            count,
            Count - count
        );
        
        Count = count;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue[] PopArguments(int count)
    {
        if (count == 0)
        {
            return [];
        }

        if (Count < count)
        {
            throw new InvalidOperationException("Stack underflow");
        }

        var args = new DynamicValue[count];
        
        Count -= count;
        _Array.Copy(
            _items,
            Count,
            args,
            0,
            count
        );
        
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

        if (Count < count)
        {
            throw new InvalidOperationException("Stack underflow");
        }

        Count -= count;
        new ReadOnlySpan<DynamicValue>(_items, Count, count).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<DynamicValue> PopSpan(int count)
    {
        if (count == 0)
        {
            return [];
        }

        if (Count < count)
        {
            throw new InvalidOperationException("Stack underflow");
        }

        Count -= count;
        return new ReadOnlySpan<DynamicValue>(_items, Count, count);
    }

}
