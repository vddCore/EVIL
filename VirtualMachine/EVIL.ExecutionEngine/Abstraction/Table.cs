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

        public bool IsSet(DynamicValue key)
            => Entries.ContainsKey(key);
        
        public DynamicValue Get(DynamicValue key)
        {
            EnsureValidKeyType(key);
            
            if (Entries.ContainsKey(key))
                return Entries[key];
            
            return DynamicValue.Zero;
        }

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