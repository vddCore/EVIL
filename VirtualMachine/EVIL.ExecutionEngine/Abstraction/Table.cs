using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace EVIL.ExecutionEngine.Abstraction
{
    public partial class Table : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
    {
        private object _lock = new();
        
        private Dictionary<DynamicValue, DynamicValue> _entries = new();

        public bool Frozen { get; private set; }
        public static readonly Table Empty = new() { Frozen = true };

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _entries.Count;
                }
            }
        }

        public DynamicValue this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public DynamicValue this[double key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public Table()
        {
        }
        
        public Table(params DynamicValue[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                Set(new DynamicValue(i), values[i]);
            }
        }

        public void Add(DynamicValue value)
        {
            lock (_lock)
            {
                Set(_entries.Count, value);
            }
        }

        public bool IsSet(string key)
            => IsSet(new DynamicValue(key));

        public bool IsSet(double key)
            => IsSet(new DynamicValue(key));

        public bool IsSet(DynamicValue key)
        {
            lock (_lock)
            {
                return _entries.ContainsKey(key);
            }
        }

        public DynamicValue Get(string key)
            => Get(new DynamicValue(key));

        public DynamicValue Get(double key)
            => Get(new DynamicValue(key));
        
        public DynamicValue Get(DynamicValue key)
        {
            lock (_lock)
            {
                EnsureValidKeyType(key);

                if (_entries.ContainsKey(key))
                    return _entries[key];

                return DynamicValue.Null;
            }
        }

        public DynamicValue Set(DynamicValue key, DynamicValue value)
        {
            lock (_lock)
            {
                if (Frozen)
                    return DynamicValue.Null;

                EnsureValidKeyType(key);
                
                if (value == DynamicValue.Null)
                {
                    _entries.Remove(key);
                    return DynamicValue.Null;
                }

                if (_entries.ContainsKey(key))
                {
                    _entries[key] = value;
                }
                else
                {
                    _entries.Add(key, value);
                }
            }

            return value;
        }

        public DynamicValue Set(string key, DynamicValue value)
            => Set(new DynamicValue(key), value);

        public DynamicValue Set(double key, DynamicValue value)
            => Set(new DynamicValue(key), value);
        
        public bool Unset(DynamicValue key)
        {
            lock (_lock)
            {
                if (Frozen)
                    return false;

                EnsureValidKeyType(key);
                return _entries.Remove(key);
            }
        }

        public bool Unset(string key)
            => Unset(new DynamicValue(key));

        public bool Unset(double key)
            => Unset(new DynamicValue(key));

        public void Freeze(bool deep)
        {
            lock (_lock)
            {
                Frozen = true;

                if (deep)
                {
                    foreach (var v in _entries.Values)
                    {
                        if (v.Type == DynamicValueType.Table)
                        {
                            v.Table.Freeze(true);
                        }
                    }
                }
            }
        }

        public static Table FromString(string str)
        {
            var t = new Table();

            for (var i = 0; i < str.Length; i++)
                t.Set(i, new(str[i].ToString()));

            return t;
        }

        [DebuggerHidden]
        private void EnsureValidKeyType(DynamicValue key)
        {
            if (key.Type != DynamicValueType.Number
                && key.Type != DynamicValueType.String)
            {
                throw new InvalidKeyTypeException(key.Type, DynamicValueType.Table);
            }
        }

        public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>> GetEnumerator()
        {
            lock (_lock)
            {
                return _entries.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}