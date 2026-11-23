namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
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

    public static readonly DynamicValue AddMetaKeyVal = new(AddMetaKey);
    public static readonly DynamicValue SubtractMetaKeyVal = new(SubtractMetaKey);
    public static readonly DynamicValue MultiplyMetaKeyVal = new(MultiplyMetaKey);
    public static readonly DynamicValue DivideMetaKeyVal = new(DivideMetaKey);
    public static readonly DynamicValue ModuloMetaKeyVal = new(ModuloMetaKey);
    public static readonly DynamicValue ShiftLeftMetaKeyVal = new(ShiftLeftMetaKey);
    public static readonly DynamicValue ShiftRightMetaKeyVal = new(ShiftRightMetaKey);
    public static readonly DynamicValue ArithmeticNegateMetaKeyVal = new(ArithmeticNegateMetaKey);
    public static readonly DynamicValue LogicalNotMetaKeyVal = new(LogicalNotMetaKey);
    public static readonly DynamicValue LogicalOrMetaKeyVal = new(LogicalOrMetaKey);
    public static readonly DynamicValue LogicalAndMetaKeyVal = new(LogicalAndMetaKey);
    public static readonly DynamicValue BitwiseOrMetaKeyVal = new(BitwiseOrMetaKey);
    public static readonly DynamicValue BitwiseXorMetaKeyVal = new(BitwiseXorMetaKey);
    public static readonly DynamicValue BitwiseAndMetaKeyVal = new(BitwiseAndMetaKey);
    public static readonly DynamicValue BitwiseNotMetaKeyVal = new(BitwiseNotMetaKey);
    public static readonly DynamicValue DeepEqualMetaKeyVal = new(DeepEqualMetaKey);
    public static readonly DynamicValue DeepNotEqualMetaKeyVal = new(DeepNotEqualMetaKey);
    public static readonly DynamicValue EqualMetaKeyVal = new(EqualMetaKey);
    public static readonly DynamicValue NotEqualMetaKeyVal = new(NotEqualMetaKey);
    public static readonly DynamicValue GreaterThanMetaKeyVal = new(GreaterThanMetaKey);
    public static readonly DynamicValue GreaterEqualMetaKeyVal = new(GreaterEqualMetaKey);
    public static readonly DynamicValue LessThanMetaKeyVal = new(LessThanMetaKey);
    public static readonly DynamicValue LessEqualMetaKeyVal = new(LessEqualMetaKey);
    public static readonly DynamicValue InvokeMetaKeyVal = new(InvokeMetaKey);
    public static readonly DynamicValue LengthMetaKeyVal = new(LengthMetaKey);
    public static readonly DynamicValue ToStringMetaKeyVal = new(ToStringMetaKey);
    public static readonly DynamicValue ToNumberMetaKeyVal = new(ToNumberMetaKey);
    public static readonly DynamicValue ExistsMetaKeyVal = new(ExistsMetaKey);
    public static readonly DynamicValue IncrementMetaKeyVal = new(IncrementMetaKey);
    public static readonly DynamicValue DecrementMetaKeyVal = new(DecrementMetaKey);
    public static readonly DynamicValue SetMetaKeyVal = new(SetMetaKey);
    public static readonly DynamicValue GetMetaKeyVal = new(GetMetaKey);

    public MetamethodFlags MetamethodFlags { get; set; }

    private const int MaxArrayPartCapacity = 65536;

    private DynamicValue[]? _arrayPart;
    private int _arrayPartCount;
    private Dictionary<DynamicValue, DynamicValue>? _hashPart;

    public TableShape? Shape { get; private set; }
    public DynamicValue[]? ShapeSlots { get; private set; }
    public uint Version { get; private set; }

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
            var count = _arrayPartCount;
            if (Shape != null && ShapeSlots != null)
            {
                for (var i = 0; i < ShapeSlots.Length; i++)
                {
                    if (ShapeSlots[i] != Nil)
                    {
                        count++;
                    }
                }
            }

            if (_hashPart != null)
            {
                count += _hashPart.Count;
            }

            return count;
        }
    }

    public Table? MetaTable { get; set; }
    public bool HasMetaTable => MetaTable != null;

    public Table()
    {
    }

    public Table(TableShape shape, DynamicValue[] shapeSlots)
    {
        Shape = shape;
        ShapeSlots = shapeSlots;
    }

    public Table(int capacity)
    {
        if (capacity > 0)
        {
            _hashPart = new(capacity);
        }
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
    }

    public static Table CreateKeyed(ReadOnlySpan<DynamicValue> items)
    {
        var pairCount = items.Length / 2;
        var allStrings = pairCount is > 0 and <= 64;
        
        if (allStrings)
        {
            for (var i = 0; i < pairCount; i++)
            {
                var k = items[i * 2 + 1];
                if (k.Type != DynamicValueType.String 
                    || (k.String is { Length: > 2 } s && s[0] == '_' && s[1] == '_'))
                {
                    allStrings = false;
                    break;
                }
            }
        }

        if (allStrings)
        {
            var shape = TableShape.Empty;
            var slots = new DynamicValue[pairCount];
            for (var i = 0; i < pairCount; i++)
            {
                var val = items[i * 2];
                var keyStr = items[i * 2 + 1].String!;
                shape = shape.Transition(keyStr, out var slot);
                if (slot >= slots.Length)
                {
                    var newSlots = new DynamicValue[shape.PropertyCount];
                    slots.CopyTo(newSlots, 0);
                    slots = newSlots;
                }
                slots[slot] = val;
            }

            return new Table(shape, slots);
        }

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
            MetamethodFlags = other.MetamethodFlags;
            Shape = other.Shape;
            Version = other.Version;

            if (other.ShapeSlots != null)
            {
                ShapeSlots = new DynamicValue[other.ShapeSlots.Length];
                System.Array.Copy(other.ShapeSlots, ShapeSlots, other.ShapeSlots.Length);
            }

            if (other._arrayPart != null)
            {
                _arrayPart = new DynamicValue[other._arrayPart.Length];
                System.Array.Copy(other._arrayPart, _arrayPart, other._arrayPart.Length);
                _arrayPartCount = other._arrayPartCount;
            }

            if (other._hashPart is { Count: > 0 })
            {
                _hashPart = new(other._hashPart.Count);
                foreach (var kvp in other._hashPart)
                {
                    _hashPart[kvp.Key] = kvp.Value;
                }
            }
        }
        else
        {
            foreach (var (key, value) in collection)
            {
                this[key] = value;
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MetamethodFlags GetMetamethodFlag(string name)
    {
        return name switch
        {
            AddMetaKey => MetamethodFlags.Add,
            SubtractMetaKey => MetamethodFlags.Sub,
            MultiplyMetaKey => MetamethodFlags.Mul,
            DivideMetaKey => MetamethodFlags.Div,
            ModuloMetaKey => MetamethodFlags.Mod,
            ShiftLeftMetaKey => MetamethodFlags.Shl,
            ShiftRightMetaKey => MetamethodFlags.Shr,
            ArithmeticNegateMetaKey => MetamethodFlags.Aneg,
            LogicalNotMetaKey => MetamethodFlags.Lnot,
            LogicalOrMetaKey => MetamethodFlags.Lor,
            LogicalAndMetaKey => MetamethodFlags.Land,
            BitwiseOrMetaKey => MetamethodFlags.Bor,
            BitwiseXorMetaKey => MetamethodFlags.Bxor,
            BitwiseAndMetaKey => MetamethodFlags.Band,
            BitwiseNotMetaKey => MetamethodFlags.Bnot,
            DeepEqualMetaKey => MetamethodFlags.Deq,
            DeepNotEqualMetaKey => MetamethodFlags.Dne,
            EqualMetaKey => MetamethodFlags.Eq,
            NotEqualMetaKey => MetamethodFlags.Ne,
            GreaterThanMetaKey => MetamethodFlags.Gt,
            GreaterEqualMetaKey => MetamethodFlags.Gte,
            LessThanMetaKey => MetamethodFlags.Lt,
            LessEqualMetaKey => MetamethodFlags.Lte,
            InvokeMetaKey => MetamethodFlags.Invoke,
            LengthMetaKey => MetamethodFlags.Len,
            ToStringMetaKey => MetamethodFlags.ToStr,
            ToNumberMetaKey => MetamethodFlags.ToNum,
            ExistsMetaKey => MetamethodFlags.Exists,
            IncrementMetaKey => MetamethodFlags.Inc,
            DecrementMetaKey => MetamethodFlags.Dec,
            SetMetaKey => MetamethodFlags.Set,
            GetMetaKey => MetamethodFlags.Get,
            _ => MetamethodFlags.None
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIntegerKey(DynamicValue key, out int index)
    {
        if (key.Type == DynamicValueType.Number)
        {
            var num = key.Number;
            if (num is >= 0 and <= int.MaxValue && (long)num == num)
            {
                index = (int)num;
                return true;
            }
        }

        index = -1;
        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int RoundUpToPowerOf2(int value)
    {
        if (value <= 1)
        {
            return 1;
        }

        return (int)BitOperations.RoundUpToPowerOf2((uint)value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementVersion()
    {
        unchecked
        {
            Version++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Dictionary<DynamicValue, DynamicValue> EnsureHashPart(int capacity = 0)
    {
        return _hashPart ??= capacity > 0
            ? new Dictionary<DynamicValue, DynamicValue>(capacity)
            : new Dictionary<DynamicValue, DynamicValue>();
    }

    private void DemoteShapeToHashPart()
    {
        if (Shape == null || ShapeSlots == null)
        {
            return;
        }

        var hash = EnsureHashPart(Shape.PropertyCount);
        var propKeys = Shape.PropertyKeys;
        
        for (var i = 0; i < ShapeSlots.Length; i++)
        {
            var val = ShapeSlots[i];
            if (val != Nil)
            {
                hash[propKeys[i]] = val;
            }
        }

        Shape = null;
        ShapeSlots = null;
        
        IncrementVersion();
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
        {
            return;
        }

        (key, value) = OnBeforeSet(key, value);

        if (key == Nil)
        {
            return;
        }

        if (value == Nil)
        {
            if (IsIntegerKey(key, out var idx) && _arrayPart != null && (uint)idx < (uint)_arrayPart.Length)
            {
                if (_arrayPart[idx] != Nil)
                {
                    _arrayPart[idx] = Nil;
                    _arrayPartCount--;
                    
                    IncrementVersion();
                }
            }

            if (Shape != null 
                && key is { Type: DynamicValueType.String, String: not null })
            {
                if (Shape.TryGetSlot(key.String, out var slot))
                {
                    if (ShapeSlots != null 
                        && slot < ShapeSlots.Length && ShapeSlots[slot] != Nil)
                    {
                        DemoteShapeToHashPart();
                        _hashPart!.Remove(key);
                        IncrementVersion();
                    }
                }
            }

            if (_hashPart is { Count: > 0 })
            {
                if (_hashPart.Remove(key))
                {
                    IncrementVersion();
                }
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
                    if (_hashPart is { Count: > 0 })
                    {
                        _hashPart.Remove(key);
                    }
                }
                else if (_arrayPart == null && index < 8)
                {
                    var initialCap = Math.Max(4, RoundUpToPowerOf2(index + 1));
                    
                    _arrayPart = new DynamicValue[initialCap];
                    System.Array.Fill(_arrayPart, Nil);
                    _arrayPart[index] = value;
                    _arrayPartCount = 1;
                    
                    if (_hashPart is { Count: > 0 })
                    {
                        _hashPart.Remove(key);
                    }
                }
                else if (_arrayPart != null && index < MaxArrayPartCapacity && index <= _arrayPart.Length * 2)
                {
                    var oldLen = _arrayPart.Length;
                    var newLen = Math.Min(
                        MaxArrayPartCapacity, 
                        RoundUpToPowerOf2(Math.Max(oldLen * 2, index + 1))
                    );
                    
                    System.Array.Resize(ref _arrayPart, newLen);
                    System.Array.Fill(_arrayPart, Nil, oldLen, newLen - oldLen);
                    
                    _arrayPart[index] = value;
                    _arrayPartCount++;
                    
                    if (_hashPart is { Count: > 0 })
                    {
                        _hashPart.Remove(key);
                    }
                }
                else
                {
                    EnsureHashPart()[key] = value;
                }

                IncrementVersion();
            }
            else if (key is { Type: DynamicValueType.String, String: not null })
            {
                var isMeta = key.String.Length > 2 && key.String[0] == '_' && key.String[1] == '_';
                if (!isMeta)
                {
                    if (Shape != null)
                    {
                        if (Shape.TryGetSlot(key.String, out var slot))
                        {
                            ShapeSlots![slot] = value;
                            IncrementVersion();
                        }
                        else if (_hashPart == null && Shape.PropertyCount < 64)
                        {
                            var nextShape = Shape.Transition(key.String, out var newSlot);
                            var newSlots = new DynamicValue[nextShape.PropertyCount];
                            
                            ShapeSlots!.CopyTo(newSlots, 0);
                            newSlots[newSlot] = value;
                            Shape = nextShape;
                            ShapeSlots = newSlots;
                            
                            IncrementVersion();
                        }
                        else
                        {
                            DemoteShapeToHashPart();
                            EnsureHashPart()[key] = value;
                            IncrementVersion();
                        }
                    }
                    else if (GetType() == typeof(Table) && _hashPart == null && (_arrayPart == null || _arrayPartCount == 0))
                    {
                        var shape = TableShape.Empty.Transition(key.String, out var slot);
                        var slots = new DynamicValue[1];
                        slots[slot] = value;
                        
                        Shape = shape;
                        ShapeSlots = slots;
                        
                        IncrementVersion();
                    }
                    else
                    {
                        EnsureHashPart()[key] = value;
                        IncrementVersion();
                    }
                }
                else
                {
                    DemoteShapeToHashPart();
                    EnsureHashPart()[key] = value;
                    IncrementVersion();
                }
            }
            else
            {
                DemoteShapeToHashPart();
                EnsureHashPart()[key] = value;
                IncrementVersion();
            }
        }

        if (key.Type == DynamicValueType.String)
        {
            var str = key.String;
            if (str is { Length: > 2 } && str[0] == '_' && str[1] == '_')
            {
                var flag = GetMetamethodFlag(str);
                if (flag != MetamethodFlags.None)
                {
                    if (value == Nil)
                    {
                        MetamethodFlags &= ~flag;
                    }
                    else
                    {
                        MetamethodFlags |= flag;
                    }
                }
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

        if (Shape != null 
            && ShapeSlots != null 
            && key is { Type: DynamicValueType.String, String: not null })
        {
            if (Shape.TryGetSlot(key.String, out var slot))
            {
                var val = ShapeSlots[slot];
                if (val != Nil)
                {
                    value = val;
                    return true;
                }
            }
        }

        if (_hashPart != null && _hashPart.TryGetValue(key, out var hashVal))
        {
            value = hashVal;
            return true;
        }

        value = Nil;
        return false;
    }

    protected virtual DynamicValue OnIndex(DynamicValue key)
    {
        return OnTryGetValue(key, out var val) 
            ? val 
            : Nil;
    }

    public bool Contains(DynamicValue key)
    {
        return OnContains(key);
    }

    protected virtual bool OnContains(DynamicValue key)
    {
        if (IsIntegerKey(key, out var index)
            && _arrayPart != null 
            && (uint)index < (uint)_arrayPart.Length)
        {
            if (_arrayPart[index] != Nil)
            {
                return true;
            }
        }

        if (Shape != null 
            && ShapeSlots != null 
            && key is { Type: DynamicValueType.String, String: not null })
        {
            if (Shape.TryGetSlot(key.String, out var slot) 
                && ShapeSlots[slot] != Nil)
            {
                return true;
            }
        }

        return _hashPart != null && _hashPart.ContainsKey(key);
    }

    public void Clear()
    {
        if (_arrayPart != null)
        {
            System.Array.Fill(_arrayPart, Nil);
            _arrayPartCount = 0;
        }

        Shape = null;
        ShapeSlots = null;
        
        _hashPart?.Clear();
        
        MetamethodFlags = MetamethodFlags.None;
        IncrementVersion();
    }

    public Table Freeze(bool deep = false)
    {
        IsFrozen = true;

        if (deep)
        {
            if (ShapeSlots != null)
            {
                for (var idx = 0; idx < ShapeSlots.Length; idx++)
                {
                    if (ShapeSlots[idx].Type == DynamicValueType.Table)
                    {
                        ShapeSlots[idx].Table!.Freeze(deep);
                    }
                }
            }

            if (_arrayPart != null)
            {
                for (var idx = 0; idx < _arrayPart.Length; idx++)
                {
                    if (_arrayPart[idx].Type == DynamicValueType.Table)
                    {
                        _arrayPart[idx].Table!.Freeze(deep);
                    }
                }
            }

            if (_hashPart != null)
            {
                foreach (var value in _hashPart.Values)
                {
                    if (value.Type == DynamicValueType.Table)
                    {
                        value.Table!.Freeze(deep);
                    }
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
            if (ShapeSlots != null)
            {
                for (var idx = 0; idx < ShapeSlots.Length; idx++)
                {
                    if (ShapeSlots[idx].Type == DynamicValueType.Table)
                    {
                        ShapeSlots[idx].Table!.Unfreeze(deep);
                    }
                }
            }

            if (_arrayPart != null)
            {
                for (var idx = 0; idx < _arrayPart.Length; idx++)
                {
                    if (_arrayPart[idx].Type == DynamicValueType.Table)
                    {
                        _arrayPart[idx].Table!.Unfreeze(deep);
                    }
                }
            }

            if (_hashPart != null)
            {
                foreach (var value in _hashPart.Values)
                {
                    if (value.Type == DynamicValueType.Table)
                    {
                        value.Table!.Unfreeze(deep);
                    }
                }
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

        if (Shape != null && ShapeSlots != null)
        {
            var propKeys = Shape.PropertyKeys;
            
            for (var idx = 0; idx < ShapeSlots.Length; idx++)
            {
                if (ShapeSlots[idx] != Nil)
                {
                    keys[i++] = propKeys[idx];
                }
            }
        }

        if (_hashPart != null)
        {
            foreach (var key in _hashPart.Keys)
            {
                keys[i++] = key;
            }
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

        if (Shape != null && ShapeSlots != null)
        {
            for (var idx = 0; idx < ShapeSlots.Length; idx++)
            {
                if (ShapeSlots[idx] != Nil)
                {
                    values[i++] = ShapeSlots[idx];
                }
            }
        }

        if (_hashPart != null)
        {
            foreach (var value in _hashPart.Values)
            {
                values[i++] = value;
            }
        }

        return values;
    }

    public Table ShallowCopy()
    {
        var copy = new Table
        {
            MetaTable = MetaTable?.ShallowCopy(),
            MetamethodFlags = MetamethodFlags,
            Shape = Shape,
            Version = Version
        };

        if (ShapeSlots != null)
        {
            copy.ShapeSlots = new DynamicValue[ShapeSlots.Length];
            System.Array.Copy(ShapeSlots, copy.ShapeSlots, ShapeSlots.Length);
        }

        if (_arrayPart != null)
        {
            copy._arrayPart = new DynamicValue[_arrayPart.Length];
            System.Array.Copy(_arrayPart, copy._arrayPart, _arrayPart.Length);
            copy._arrayPartCount = _arrayPartCount;
        }

        if (_hashPart is { Count: > 0 })
        {
            copy._hashPart = new Dictionary<DynamicValue, DynamicValue>(_hashPart.Count);
            foreach (var kvp in _hashPart)
            {
                copy._hashPart[kvp.Key] = kvp.Value;
            }
        }

        return copy;
    }

    public Table DeepCopy()
    {
        var copy = new Table
        {
            MetaTable = MetaTable?.DeepCopy(),
            MetamethodFlags = MetamethodFlags,
            Shape = Shape,
            Version = Version
        };

        if (ShapeSlots != null)
        {
            copy.ShapeSlots = new DynamicValue[ShapeSlots.Length];
            for (var i = 0; i < ShapeSlots.Length; i++)
            {
                var val = ShapeSlots[i];
                copy.ShapeSlots[i] = val.Type == DynamicValueType.Table ? val.Table!.DeepCopy() : val;
            }
        }

        if (_arrayPart != null)
        {
            copy._arrayPart = new DynamicValue[_arrayPart.Length];
            copy._arrayPartCount = _arrayPartCount;
            for (var i = 0; i < _arrayPart.Length; i++)
            {
                var val = _arrayPart[i];
                copy._arrayPart[i] = val.Type == DynamicValueType.Table ? val.Table!.DeepCopy() : val;
            }
        }

        if (_hashPart is { Count: > 0 })
        {
            copy._hashPart = new Dictionary<DynamicValue, DynamicValue>(_hashPart.Count);
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

        if (Shape != null && ShapeSlots != null)
        {
            var keys = Shape.PropertyKeys;
            for (var i = 0; i < ShapeSlots.Length; i++)
            {
                var val = ShapeSlots[i];
                if (val != Nil)
                {
                    yield return new KeyValuePair<DynamicValue, DynamicValue>(
                        keys[i],
                        val
                    );
                }
            }
        }

        if (_hashPart != null)
        {
            foreach (var kvp in _hashPart)
            {
                yield return kvp;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
