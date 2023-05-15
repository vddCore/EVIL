using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class ChunkAttribute : IEquatable<ChunkAttribute>
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

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj)
                   || obj is ChunkAttribute other
                   && Equals(other);
        }

        public override int GetHashCode() 
            => HashCode.Combine(Name, Values, Properties);

        public static bool operator ==(ChunkAttribute? left, ChunkAttribute? right) 
            => Equals(left, right);

        public static bool operator !=(ChunkAttribute? left, ChunkAttribute? right) 
            => !Equals(left, right);
    }
}