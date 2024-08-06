namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Marshaling;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public sealed record ChunkAttribute : IDynamicValueProvider
{
    public string Name { get; }

    public List<DynamicValue> Values { get; } = new();
    public Dictionary<string, DynamicValue> Properties { get; } = new();

    public DynamicValue this[int valueIndex]
    {
        get
        {
            if (valueIndex < 0 || valueIndex >= Values.Count)
            {
                return DynamicValue.Nil;
            }

            return Values[valueIndex];
        }
    }

    public DynamicValue this[string propertyKey]
    {
        get
        {
            if (Properties.TryGetValue(propertyKey, out var value))
            {
                return value;
            }

            return DynamicValue.Nil;
        }
    }

    public ChunkAttribute(string name)
    {
        Name = name;
    }

    public bool Equals(ChunkAttribute? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name
               && Values.SequenceEqual(other.Values)
               && Properties.SequenceEqual(other.Properties);
    }

    public override int GetHashCode() 
        => HashCode.Combine(Name, Values, Properties);

    public DynamicValue ToDynamicValue()
    {
        var values = new Table();
        for (var i = 0; i < Values.Count; i++)
        {
            values[i] = Values[i];
        }

        var properties = new Table();
        foreach (var kvp in Properties)
        {
            properties[kvp.Key] = kvp.Value;
        }

        return new Table
        {
            { "name", Name },
            { "values", values },
            { "properties", properties }
        };
    }
}