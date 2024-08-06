namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

internal sealed class ArrayEnumerator : IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>, ICloneable
{
    private readonly Array _array;
    private int _index;
        
    object IEnumerator.Current => Current;
        
    public KeyValuePair<DynamicValue, DynamicValue> Current
    {
        get
        {
            var index = _index;
            var array = _array;

            if (index >= array.Length)
            {
                if (index < 0)
                {
                    throw new InvalidOperationException("Enumeration has not started yet.");
                }
                else
                {
                    throw new InvalidOperationException("Enumeration has already finished.");
                }
            }

            return new(index, array[index]);
        }
    }

    internal ArrayEnumerator(Array array)
    {
        _array = array;
        _index = -1;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public bool MoveNext()
    {
        var index = _index + 1;
            
        if (index >= _array.Length)
        {
            _index = _array.Length;
            return false;
        }
            
        _index = index;
        return true;
    }

    public void Reset()
    {
        _index = -1;
    }

    public void Dispose()
    {
    }
}