using System.Collections.Generic;
using System.Linq;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class StringPool
    {
        private Dictionary<int, string> _forward = new();
        private Dictionary<string, int> _backward = new();

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
    }
}