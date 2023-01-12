using System.Collections.Generic;
using System.Diagnostics;

namespace EVIL.ExecutionEngine.Abstraction
{
    public class Table
    {
        public Dictionary<DynamicValue, DynamicValue> Entries = new();

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
            => Entries.ContainsKey(key);

        public DynamicValue Get(string key)
            => Get(new DynamicValue(key));

        public DynamicValue Get(double key)
            => Get(new DynamicValue(key));
        
        public DynamicValue Get(DynamicValue key)
        {
            EnsureValidKeyType(key);
            
            if (Entries.ContainsKey(key))
                return Entries[key];
            
            return DynamicValue.Zero;
        }

        public void SetByString(string key, DynamicValue value)
            => Set(new DynamicValue(key), value);

        public void SetByNumber(double key, DynamicValue value)
            => Set(new DynamicValue(key), value);

        public void Set(DynamicValue key, DynamicValue value)
        {
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

        public bool Unset(DynamicValue key)
        {
            EnsureValidKeyType(key);
            return Entries.Remove(key);
        }

        public static Table FromString(string str)
        {
            var t = new Table();

            for (var i = 0; i < str.Length; i++)
                t.Set(new(i), new(str[i].ToString()));

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
    }
}