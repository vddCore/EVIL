using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EVIL.ExecutionEngine.Abstraction
{
    public partial class Table
    {
        private object _lock = new();
        
        public Dictionary<DynamicValue, DynamicValue> Entries = new();

        public bool Frozen { get; private set; }
        public static readonly Table Empty = new() { Frozen = true };

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

        public bool IsSet(string key)
            => IsSet(new DynamicValue(key));

        public bool IsSet(double key)
            => IsSet(new DynamicValue(key));

        public bool IsSet(DynamicValue key)
        {
            lock (_lock)
            {
                return Entries.ContainsKey(key);
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

                if (Entries.ContainsKey(key))
                    return Entries[key];

                return DynamicValue.Null;
            }
        }

        public void Set(DynamicValue key, DynamicValue value)
        {
            lock (_lock)
            {
                if (Frozen)
                    return;

                EnsureValidKeyType(key);

                if (Entries.ContainsKey(key))
                {
                    Entries[key] = value;
                }
                else
                {
                    Entries.Add(key, value);
                }
            }
        }

        public void Set(string key, DynamicValue value)
            => Set(new DynamicValue(key), value);

        public void Set(double key, DynamicValue value)
            => Set(new DynamicValue(key), value);
        
        public bool Unset(DynamicValue key)
        {
            lock (_lock)
            {
                if (Frozen)
                    return false;

                EnsureValidKeyType(key);
                return Entries.Remove(key);
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
                    foreach (var v in Entries.Values)
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
                throw new InvalidKeyTypeException(DynamicValueType.Table, key.Type);
            }
        }
    }
}