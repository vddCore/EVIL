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
            throw new InvalidOperationException("Stack is empty");
            
        return _items[--_count];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out DynamicValue value)
    {
        value = DynamicValue.Nil;

        if (_count == 0)
            return false;
            
        value = _items[--_count];
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("Stack is empty");
            
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
    
    public int Count => _count;
}
