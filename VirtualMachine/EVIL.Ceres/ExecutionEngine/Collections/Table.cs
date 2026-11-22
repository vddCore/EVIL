namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Collections.Serialization;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;
using static TypeSystem.DynamicValue;

public class Table : IDynamicValueCollection, IIndexableObject, IWriteableObject
{
    public const string AddMetaKey = "__add";
    public const string SubtractMetaKey = "__sub";
    public const string MultiplyMetaKey = "__mul";
    public const string DivideMetaKey = "__div";
    public const string ModuloMetaKey = "__mod";
    public const string ShiftLeftMetaKey = "__shl";
    public const string ShiftRightMetaKey = "__shr";
    public const string ArithmeticNegateMetaKey = "__aneg";
    public const string LogicalNotMetaKey = "__lnot";
    public const string LogicalOrMetaKey = "__lor";
    public const string LogicalAndMetaKey = "__land";
    public const string BitwiseOrMetaKey = "__bor";
    public const string BitwiseXorMetaKey = "__bxor";
    public const string BitwiseAndMetaKey = "__band";
    public const string BitwiseNotMetaKey = "__bnot";
    public const string DeepEqualMetaKey = "__deq";
    public const string DeepNotEqualMetaKey = "__dne";
    public const string EqualMetaKey = "__eq";
    public const string NotEqualMetaKey = "__ne";
    public const string GreaterThanMetaKey = "__gt";
    public const string GreaterEqualMetaKey = "__gte";
    public const string LessThanMetaKey = "__lt";
    public const string LessEqualMetaKey = "__lte";
    public const string InvokeMetaKey = "__invoke";
    public const string LengthMetaKey = "__len";
    public const string ToStringMetaKey = "__tostr";
    public const string ToNumberMetaKey = "__tonum";
    public const string ExistsMetaKey = "__exists";
    public const string IncrementMetaKey = "__inc";
    public const string DecrementMetaKey = "__dec";
    public const string SetMetaKey = "__set";
    public const string GetMetaKey = "__get";

    private const int MaxArrayPartCapacity = 1024;

    private DynamicValue[]? _arrayPart;
    private int _arrayPartCount;
    private readonly Dictionary<DynamicValue, DynamicValue> _hashPart;

    public DynamicValue this[DynamicValue key]
    {
        get => Index(key);
        set => Set(key, value);
    }

    public bool IsFrozen { get; private set; }

    public int Length => _arrayPartCount + _hashPart.Count;

    public Table? MetaTable { get; set; }
    public bool HasMetaTable => MetaTable != null;

    public Table()
    {
        _hashPart = new();
    }

    public Table(int capacity)
    {
        _hashPart = new(capacity);
    }

    public Table(DynamicValue[] arrayPart)
    {
        _arrayPart = arrayPart;
        var count = 0;
        for (var i = 0; i < arrayPart.Length; i++)
        {
            if (arrayPart[i] != Nil)
            {
                count++;
            }
        }

        _arrayPartCount = count;
        _hashPart = new();
    }

    public Table(ReadOnlySpan<DynamicValue> arrayPart)
    {
        var len = arrayPart.Length;
        _arrayPart = new DynamicValue[len];
        arrayPart.CopyTo(_arrayPart);
        var count = 0;
        
        for (var i = 0; i < len; i++)
        {
            if (_arrayPart[i] != Nil)
            {
                count++;
            }
        }

        _arrayPartCount = count;
        _hashPart = new();
    }

    public static Table CreateKeyed(ReadOnlySpan<DynamicValue> items)
    {
        var pairCount = items.Length / 2;
        var table = new Table(pairCount);
        for (var i = 0; i < pairCount; i++)
        {
            var val = items[i * 2];
            var key = items[i * 2 + 1];
            table.Set(key, val);
        }

        return table;
    }

    public Table(IDynamicValueCollection collection)
    {
        if (collection is Table other)
        {
            MetaTable = other.MetaTable?.ShallowCopy();
            IsFrozen = other.IsFrozen;

            if (other._arrayPart != null)
            {
                _arrayPart = new DynamicValue[other._arrayPart.Length];
                System.Array.Copy(other._arrayPart, _arrayPart, other._arrayPart.Length);
                _arrayPartCount = other._arrayPartCount;
            }

            _hashPart = new(other._hashPart.Count);
            foreach (var kvp in other._hashPart)
            {
                _hashPart[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            _hashPart = new();
            foreach (var (key, value) in collection)
            {
                this[key] = value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIntegerKey(DynamicValue key, out int index)
    {
        if (key.Type == DynamicValueType.Number)
        {
            var num = key.Number;
            if (num >= 0 && num <= int.MaxValue && (long)num == num)
            {
                index = (int)num;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public void Serialize(Stream stream)
        => TableSerializer.Serialize(this, stream);

    public static Table Deserialize(Stream stream)
        => TableSerializer.Deserialize(stream);

    public void Add(DynamicValue key, DynamicValue value)
        => Set(key, value);

    public void Set(DynamicValue key, DynamicValue value)
    {
        if (IsFrozen)
            return;

        (key, value) = OnBeforeSet(key, value);

        if (key == Nil)
            return;

        if (value == Nil)
        {
            if (IsIntegerKey(key, out var idx) && _arrayPart != null && (uint)idx < (uint)_arrayPart.Length)
            {
                if (_arrayPart[idx] != Nil)
                {
                    _arrayPart[idx] = Nil;
                    _arrayPartCount--;
                }
            }

            if (_hashPart.Count > 0)
            {
                _hashPart.Remove(key);
            }
        }
        else
        {
            if (IsIntegerKey(key, out var index))
            {
                if (_arrayPart != null && (uint)index < (uint)_arrayPart.Length)
                {
                    if (_arrayPart[index] == Nil)
                    {
                        _arrayPartCount++;
                    }

                    _arrayPart[index] = value;
                    if (_hashPart.Count > 0)
                    {
                        _hashPart.Remove(key);
                    }
                }
                else if (_arrayPart == null && index < 4)
                {
                    _arrayPart = new DynamicValue[4];
                    System.Array.Fill(_arrayPart, Nil);
                    _arrayPart[index] = value;
                    _arrayPartCount = 1;
                    if (_hashPart.Count > 0)
                    {
                        _hashPart.Remove(key);
                    }
                }
                else if (_arrayPart != null && index < MaxArrayPartCapacity && index <= _arrayPart.Length * 2)
                {
                    var oldLen = _arrayPart.Length;
                    var newLen = Math.Max(oldLen * 2, index + 1);
                    System.Array.Resize(ref _arrayPart, newLen);
                    System.Array.Fill(_arrayPart, Nil, oldLen, newLen - oldLen);
                    _arrayPart[index] = value;
                    _arrayPartCount++;
                    if (_hashPart.Count > 0)
                    {
                        _hashPart.Remove(key);
                    }
                }
                else
                {
                    _hashPart[key] = value;
                }
            }
            else
            {
                _hashPart[key] = value;
            }
        }

        OnAfterSet(key, value);
    }

    protected virtual (DynamicValue Key, DynamicValue Value) OnBeforeSet(DynamicValue key, DynamicValue value)
    {
        return (key, value);
    }

    protected virtual void OnAfterSet(DynamicValue key, DynamicValue value)
    {
    }

    public DynamicValue Index(DynamicValue key)
        => OnIndex(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(DynamicValue key, out DynamicValue value)
        => OnTryGetValue(key, out value);

    protected virtual bool OnTryGetValue(DynamicValue key, out DynamicValue value)
    {
        if (IsIntegerKey(key, out var index) && _arrayPart != null && (uint)index < (uint)_arrayPart.Length)
        {
            var val = _arrayPart[index];
            if (val != Nil)
            {
                value = val;
                return true;
            }
        }

        if (_hashPart.TryGetValue(key, out var hashVal))
        {
            value = hashVal;
            return true;
        }

        value = Nil;
        return false;
    }

    protected virtual DynamicValue OnIndex(DynamicValue key)
    {
        if (OnTryGetValue(key, out var val))
            return val;

        return Nil;
    }

    public bool Contains(DynamicValue key)
    {
        return OnContains(key);
    }

    protected virtual bool OnContains(DynamicValue key)
    {
        if (IsIntegerKey(key, out var index) && _arrayPart != null && (uint)index < (uint)_arrayPart.Length)
        {
            if (_arrayPart[index] != Nil)
                return true;
        }

        return _hashPart.ContainsKey(key);
    }

    public void Clear()
    {
        if (_arrayPart != null)
        {
            System.Array.Fill(_arrayPart, Nil);
            _arrayPartCount = 0;
        }

        _hashPart.Clear();
    }

    public Table Freeze(bool deep = false)
    {
        IsFrozen = true;

        if (deep)
        {
            if (_arrayPart != null)
            {
                for (var idx = 0; idx < _arrayPart.Length; idx++)
                {
                    if (_arrayPart[idx].Type == DynamicValueType.Table)
                        _arrayPart[idx].Table!.Freeze(deep);
                }
            }

            foreach (var value in _hashPart.Values)
            {
                if (value.Type == DynamicValueType.Table)
                    value.Table!.Freeze(deep);
            }
        }

        return this;
    }

    public Table Unfreeze(bool deep = false)
    {
        IsFrozen = false;

        if (deep)
        {
            if (_arrayPart != null)
            {
                for (var idx = 0; idx < _arrayPart.Length; idx++)
                {
                    if (_arrayPart[idx].Type == DynamicValueType.Table)
                        _arrayPart[idx].Table!.Unfreeze(deep);
                }
            }

            foreach (var value in _hashPart.Values)
            {
                if (value.Type == DynamicValueType.Table)
                    value.Table!.Unfreeze(deep);
            }
        }

        return this;
    }

    public Array GetKeys()
    {
        var keys = new Array(Length);
        var i = 0;

        if (_arrayPart != null && _arrayPartCount > 0)
        {
            for (var idx = 0; idx < _arrayPart.Length; idx++)
            {
                if (_arrayPart[idx] != Nil)
                {
                    keys[i++] = (double)idx;
                }
            }
        }

        foreach (var key in _hashPart.Keys)
        {
            keys[i++] = key;
        }

        return keys;
    }

    public Array GetValues()
    {
        var values = new Array(Length);
        var i = 0;

        if (_arrayPart != null && _arrayPartCount > 0)
        {
            for (var idx = 0; idx < _arrayPart.Length; idx++)
            {
                if (_arrayPart[idx] != Nil)
                {
                    values[i++] = _arrayPart[idx];
                }
            }
        }

        foreach (var value in _hashPart.Values)
        {
            values[i++] = value;
        }

        return values;
    }

    public Table ShallowCopy()
    {
        var copy = new Table
        {
            MetaTable = MetaTable?.ShallowCopy()
        };

        if (_arrayPart != null)
        {
            copy._arrayPart = new DynamicValue[_arrayPart.Length];
            System.Array.Copy(_arrayPart, copy._arrayPart, _arrayPart.Length);
            copy._arrayPartCount = _arrayPartCount;
        }

        foreach (var kvp in _hashPart)
        {
            copy._hashPart[kvp.Key] = kvp.Value;
        }

        return copy;
    }

    public Table DeepCopy()
    {
        var copy = new Table
        {
            MetaTable = MetaTable?.DeepCopy()
        };

        if (_arrayPart != null)
        {
            copy._arrayPart = new DynamicValue[_arrayPart.Length];
            copy._arrayPartCount = _arrayPartCount;
            for (var i = 0; i < _arrayPart.Length; i++)
            {
                var val = _arrayPart[i];
                if (val.Type == DynamicValueType.Table)
                {
                    copy._arrayPart[i] = val.Table!.DeepCopy();
                }
                else
                {
                    copy._arrayPart[i] = val;
                }
            }
        }

        foreach (var kvp in _hashPart)
        {
            if (kvp.Value.Type == DynamicValueType.Table)
            {
                copy._hashPart[kvp.Key] = kvp.Value.Table!.DeepCopy();
            }
            else
            {
                copy._hashPart[kvp.Key] = kvp.Value;
            }
        }

        return copy;
    }

    public bool IsDeeplyEqualTo(Table other)
    {
        if (Length != other.Length)
        {
            return false;
        }

        foreach (var (key, val) in this)
        {
            if (!other.TryGetValue(key, out var otherVal))
            {
                return false;
            }

            if (val.Type == DynamicValueType.Table)
            {
                if (otherVal.Type != DynamicValueType.Table 
                    || !IsTruth(val.IsDeeplyEqualTo(otherVal)))
                {
                    return false;
                }
            }
            else
            {
                if (!IsTruth(val.IsEqualTo(otherVal)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public DynamicValue IndexUsingFullyQualifiedName(string fullyQualifiedName)
    {
        var segments = fullyQualifiedName.Split('.');
        var currentTable = this;
        var ret = Nil;

        for (var i = 0; i < segments.Length; i++)
        {
            ret = currentTable[segments[i]];

            if (ret == Nil)
            {
                return ret;
            }

            if (ret.Type != DynamicValueType.Table && i + 1 < segments.Length)
            {
                return ret;
            }

            currentTable = ret.Table!;
        }

        return ret;
    }

    public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>> GetEnumerator()
    {
        if (_arrayPart != null && _arrayPartCount > 0)
        {
            for (var idx = 0; idx < _arrayPart.Length; idx++)
            {
                if (_arrayPart[idx] != Nil)
                {
                    yield return new KeyValuePair<DynamicValue, DynamicValue>(
                        (double)idx,
                        _arrayPart[idx]
                    );
                }
            }
        }

        foreach (var kvp in _hashPart)
        {
            yield return kvp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
