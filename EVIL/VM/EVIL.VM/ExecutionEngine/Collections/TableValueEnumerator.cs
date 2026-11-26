namespace EVIL.VirtualMachine.ExecutionEngine.Collections;

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public struct TableValueEnumerator : IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>
{
    private readonly Table _table;
    private int _arrayIndex;
    private int _shapeIndex;
    private Dictionary<DynamicValue, DynamicValue>.Enumerator _dictEnumerator;
    
    /**
     * 0: array part,
     * 1: shape slots,
     * 2: hash part,
     * 3: completed
     **/
    private byte _phase; 

    
    public DynamicValue CurrentKey { get; private set; }
    public DynamicValue CurrentValue { get; private set; }
    public readonly KeyValuePair<DynamicValue, DynamicValue> Current => new(CurrentKey, CurrentValue);

    readonly object IEnumerator.Current => Current;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TableValueEnumerator(Table table)
    {
        _table = table;
        _arrayIndex = 0;
        _shapeIndex = 0;
        _dictEnumerator = default(Dictionary<DynamicValue, DynamicValue>.Enumerator);
        _phase = 0;
        CurrentKey = DynamicValue.Nil;
        CurrentValue = DynamicValue.Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        if (_phase == 0)
        {
            var arr = _table.ArrayPart;
            if (arr != null && _table.ArrayPartCount > 0)
            {
                while ((uint)_arrayIndex < (uint)arr.Length)
                {
                    var val = arr[_arrayIndex];
                    var idx = _arrayIndex++;
                    
                    if (val != DynamicValue.Nil)
                    {
                        CurrentKey = (double)idx;
                        CurrentValue = val;
                        return true;
                    }
                }
            }

            _phase = 1;
        }

        if (_phase == 1)
        {
            var shape = _table.Shape;
            var slots = _table.ShapeSlots;
            if (shape != null && slots != null)
            {
                var keys = shape.PropertyKeys;
                while ((uint)_shapeIndex < (uint)slots.Length)
                {
                    var val = slots[_shapeIndex];
                    var idx = _shapeIndex++;
                    
                    if (val != DynamicValue.Nil)
                    {
                        CurrentKey = keys[idx];
                        CurrentValue = val;
                        return true;
                    }
                }
            }

            _phase = 2;
            var hash = _table.HashPart;
            if (hash is { Count: > 0 })
            {
                _dictEnumerator = hash.GetEnumerator();
            }
            else
            {
                _phase = 3;
                CurrentKey = DynamicValue.Nil;
                CurrentValue = DynamicValue.Nil;
                return false;
            }
        }

        if (_phase == 2)
        {
            if (_dictEnumerator.MoveNext())
            {
                var entry = _dictEnumerator.Current;
                CurrentKey = entry.Key;
                CurrentValue = entry.Value;
                return true;
            }

            _phase = 3;
        }

        CurrentKey = DynamicValue.Nil;
        CurrentValue = DynamicValue.Nil;
        return false;
    }

    public void Reset()
    {
        _arrayIndex = 0;
        _shapeIndex = 0;
        _dictEnumerator = default(Dictionary<DynamicValue, DynamicValue>.Enumerator);
        _phase = 0;
        CurrentKey = DynamicValue.Nil;
        CurrentValue = DynamicValue.Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_phase >= 2)
        {
            _dictEnumerator.Dispose();
        }
    }
}
