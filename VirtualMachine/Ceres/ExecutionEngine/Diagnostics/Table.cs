using System.Collections.Concurrent;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public class Table
    {
        private ConcurrentDictionary<DynamicValue, DynamicValue> _values = new();

        public DynamicValue this[string key]
        {
            get => Index(key);
            set => Set(key, value);
        }

        public DynamicValue this[double key]
        {
            get => Index(key);
            set => Set(key, value);
        }

        public DynamicValue this[DynamicValue key]
        {
            get => Index(key);
            set => Set(key, value);
        }

        public double Length => _values.Count;

        public void Set(double key, DynamicValue value)
            => Set(new DynamicValue(key), value);

        public void Set(string key, DynamicValue value)
            => Set(new DynamicValue(key), value);

        public void Set(DynamicValue key, DynamicValue value)
        {
            if (value == DynamicValue.Nil)
            {
                _values.TryRemove(key, out _);
            }
            else
            {
                _values.AddOrUpdate(key, (_) => value, (_, _) => value);
            }
        }

        public DynamicValue Index(double key)
            => Index(new DynamicValue(key));

        public DynamicValue Index(string key)
            => Index(new DynamicValue(key));

        public DynamicValue Index(DynamicValue key)
        {
            if (!_values.TryGetValue(key, out var value))
                return DynamicValue.Nil;

            return value;
        }

        public bool Contains(string key)
            => Contains(new DynamicValue(key));

        public bool Contains(double key)
            => Contains(new DynamicValue(key));

        public bool Contains(DynamicValue key)
            => _values.ContainsKey(key);

        public void Clear()
            => _values.Clear();

        public Table GetKeys()
        {
            var keys = new Table();

            for (var i = 0; i < _values.Keys.Count; i++)
                keys.Set(i, _values.Keys.ElementAt(i));

            return keys;
        }
    }
}