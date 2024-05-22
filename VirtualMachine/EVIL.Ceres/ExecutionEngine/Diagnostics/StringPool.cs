using System;
using System.Collections.Generic;
using System.Linq;

namespace EVIL.Ceres.ExecutionEngine.Diagnostics
{
    public sealed class StringPool : IEquatable<StringPool>
    {
        private readonly Dictionary<int, string> _forward = new();
        private readonly Dictionary<string, int> _backward = new();

        public int Count => _forward.Count;

        public string? this[int id]
        {
            get
            {
                if (!_forward.TryGetValue(id, out var str))
                    return null;

                return str;
            }
        }

        public StringPool()
        {
        }

        public StringPool(string[] values)
        {
            for (var i = 0; i < values.Length; i++)
                FetchOrAdd(values[i]);
        }

        public long FetchOrAdd(string value)
        {
            if (_backward.TryGetValue(value, out int id))
                return id;

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
}