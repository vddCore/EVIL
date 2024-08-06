namespace EVIL.Ceres.ExecutionEngine.Collections;

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
    private ConcurrentDictionary<DynamicValue, DynamicValue> _values = new();

    public DynamicValue this[DynamicValue key]
    {
        get => Index(key);
        set => Set(key, value);
    }

    public bool IsFrozen { get; private set; }

    public int Length
    {
        get
        {
            lock (_values)
            {
                return _values.Count;
            }
        }
    }

    public Table? MetaTable { get; set; }
    public bool HasMetaTable => MetaTable != null;

    public Table()
    {
    }

    public Table(IDynamicValueCollection collection)
    {
        foreach (var (key, value) in collection)
        {
            this[key] = value;
        }
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

        lock (_values)
        {
            if (value == Nil)
            {
                _values.TryRemove(key, out _);
            }
            else
            {
                _values.AddOrUpdate(key, (_) => value, (_, _) => value);
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

    protected virtual DynamicValue OnIndex(DynamicValue key)
    {
        lock (_values)
        {
            if (!_values.TryGetValue(key, out var value))
                return Nil;

            return value;
        }
    }

    public bool Contains(DynamicValue key)
    {
        return OnContains(key);
    }

    protected virtual bool OnContains(DynamicValue key)
    {
        lock (_values)
        {
            return _values.ContainsKey(key);
        }
    }

    public void Clear()
    {
        lock (_values)
        {
            _values.Clear();
        }
    }

    public Table Freeze(bool deep = false)
    {
        IsFrozen = true;

        if (deep)
        {
            lock (_values)
            {
                foreach (var value in _values.Values)
                {
                    if (value.Type == DynamicValueType.Table)
                        value.Table!.Freeze(deep);
                }
            }
        }

        return this;
    }

    public Table Unfreeze(bool deep = false)
    {
        IsFrozen = false;

        if (deep)
        {
            lock (_values)
            {
                foreach (var value in _values.Values)
                {
                    if (value.Type == DynamicValueType.Table)
                        value.Table!.Freeze();
                }
            }
        }

        return this;
    }

    public Array GetKeys()
    {
        lock (_values)
        {
            var keys = new Array(_values.Count);
            var i = 0;

            foreach (var kvp in _values)
            {
                keys[i++] = kvp.Key;
            }

            return keys;
        }
    }

    public Array GetValues()
    {
        lock (_values)
        {
            var values = new Array(_values.Count);
            var i = 0;

            foreach (var kvp in _values)
            {
                values[i++] = kvp.Value;
            }

            return values;
        }
    }

    public Table ShallowCopy()
    {
        var copy = new Table();

        foreach (var kvp in this)
            copy[kvp.Key] = kvp.Value;

        return copy;
    }

    public Table DeepCopy()
    {
        var copy = new Table();

        copy.MetaTable = MetaTable?.ShallowCopy();

        foreach (var kvp in this)
        {
            if (kvp.Value.Type == DynamicValueType.Table)
            {
                copy[kvp.Key] = kvp.Value.Table!.DeepCopy();
            }
            else
            {
                copy[kvp.Key] = kvp.Value;
            }
        }

        return copy;
    }

    public bool IsDeeplyEqualTo(Table other)
    {
        if (Length != other.Length)
            return false;

        lock (_values)
        {
            for (var i = 0; i < _values.Keys.Count; i++)
            {
                var k = _values.Keys.ElementAt(i);

                if (this[k].Type == DynamicValueType.Table)
                {
                    if (!IsTruth(this[k].IsDeeplyEqualTo(other[k])))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!IsTruth(this[k].IsEqualTo(other[k])))
                    {
                        return false;
                    }
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

        lock (_values)
        {
            for (var i = 0; i < segments.Length; i++)
            {
                ret = currentTable[segments[i]];

                if (ret == Nil)
                    return ret;

                if (ret.Type != DynamicValueType.Table && i + 1 < segments.Length)
                    return ret;

                currentTable = ret.Table!;
            }
        }

        return ret;
    }

    public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>> GetEnumerator()
    {
        lock (_values)
        {
            return _values.GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}