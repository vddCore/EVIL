namespace EVIL.VM.ExecutionEngine.Collections;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVIL.VM.ExecutionEngine.TypeSystem;

public struct ValueEnumerator
{
    private readonly Array? _array;
    private int _arrayIndex;
    
    private readonly EnumeratorKind _kind;
    private TableValueEnumerator _tableEnumerator;
    private readonly IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>? _customEnumerator;

    public DynamicValue CurrentKey { get; private set; }
    public DynamicValue CurrentValue { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueEnumerator(Table table)
    {
        _kind = EnumeratorKind.Table;
        _tableEnumerator = new TableValueEnumerator(table);
        _array = null;
        _arrayIndex = 0;
        _customEnumerator = null;
        
        CurrentKey = DynamicValue.Nil;
        CurrentValue = DynamicValue.Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueEnumerator(Array array)
    {
        _kind = EnumeratorKind.Array;
        _tableEnumerator = default(TableValueEnumerator);
        _array = array;
        _arrayIndex = 0;
        _customEnumerator = null;
        
        CurrentKey = DynamicValue.Nil;
        CurrentValue = DynamicValue.Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueEnumerator(IEnumerator<KeyValuePair<DynamicValue, DynamicValue>> custom)
    {
        _kind = EnumeratorKind.Custom;
        _tableEnumerator = default(TableValueEnumerator);
        _array = null;
        _arrayIndex = 0;
        _customEnumerator = custom;
        
        CurrentKey = DynamicValue.Nil;
        CurrentValue = DynamicValue.Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        switch (_kind)
        {
            case EnumeratorKind.Table:
            {
                if (_tableEnumerator.MoveNext())
                {
                    CurrentKey = _tableEnumerator.CurrentKey;
                    CurrentValue = _tableEnumerator.CurrentValue;
                    return true;
                }

                CurrentKey = DynamicValue.Nil;
                CurrentValue = DynamicValue.Nil;
                return false;
            }

            case EnumeratorKind.Array:
            {
                if (_array != null && (uint)_arrayIndex < (uint)_array.Length)
                {
                    CurrentKey = (double)_arrayIndex;
                    CurrentValue = _array[_arrayIndex];
                    _arrayIndex++;
                    return true;
                }

                CurrentKey = DynamicValue.Nil;
                CurrentValue = DynamicValue.Nil;
                return false;
            }

            case EnumeratorKind.Custom:
            {
                if (_customEnumerator != null && _customEnumerator.MoveNext())
                {
                    var cur = _customEnumerator.Current;
                    CurrentKey = cur.Key;
                    CurrentValue = cur.Value;
                    return true;
                }

                CurrentKey = DynamicValue.Nil;
                CurrentValue = DynamicValue.Nil;
                return false;
            }

            default:
            {
                CurrentKey = DynamicValue.Nil;
                CurrentValue = DynamicValue.Nil;
                return false;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_kind == EnumeratorKind.Table)
        {
            _tableEnumerator.Dispose();
        }
        else if (_kind == EnumeratorKind.Custom)
        {
            _customEnumerator?.Dispose();
        }
    }
}
