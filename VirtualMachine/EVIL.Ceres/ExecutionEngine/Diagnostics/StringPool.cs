namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public sealed class StringPool : IEquatable<StringPool>
{
    private readonly Dictionary<int, string> _forward = new();
    private readonly Dictionary<string, int> _backward = new();
    private DynamicValue[]? _dynamicValues;

    public int Count => _forward.Count;

    public string? this[int id] => _forward.GetValueOrDefault(id);

    public DynamicValue GetDynamicValue(int id)
    {
        if (_dynamicValues != null && (uint)id < (uint)_dynamicValues.Length)
        {
            var cached = _dynamicValues[id];
            if (cached != DynamicValue.Nil)
            {
                return cached;
            }
        }

        return EnsureDynamicValue(id);
    }

    private DynamicValue EnsureDynamicValue(int id)
    {
        var str = this[id];
        if (str == null)
        {
            return DynamicValue.Nil;
        }

        if (_dynamicValues == null || id >= _dynamicValues.Length)
        {
            var newArr = new DynamicValue[Math.Max(id + 1, _forward.Count)];
            _dynamicValues?.CopyTo(newArr, 0);
            _dynamicValues = newArr;
        }

        var dv = new DynamicValue(str);
        _dynamicValues[id] = dv;
        return dv;
    }

    public StringPool()
    {
    }

    public StringPool(string[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            FetchOrAdd(values[i]);
        }
    }

    public long FetchOrAdd(string value)
    {
        if (_backward.TryGetValue(value, out int id))
        {
            return id;
        }

        id = _forward.Count;

        _forward.Add(id, value);
        _backward.Add(value, id);

        return id;
    }

    public string[] ToArray()
        => _forward.Values.ToArray();

    public bool Equals(StringPool? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return _forward.SequenceEqual(other._forward)
               && _backward.SequenceEqual(other._backward);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj)
               || obj is StringPool other && Equals(other);
    }

    public override int GetHashCode()
        => HashCode.Combine(_forward, _backward);

    public static bool operator ==(StringPool? left, StringPool? right)
        => Equals(left, right);

    public static bool operator !=(StringPool? left, StringPool? right)
        => !Equals(left, right);
}