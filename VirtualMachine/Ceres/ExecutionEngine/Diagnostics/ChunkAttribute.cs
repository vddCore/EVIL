using System.Collections.Generic;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class ChunkAttribute
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
    }
}