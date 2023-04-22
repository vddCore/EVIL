using System.Collections.Generic;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class ChunkAttribute
    {
        public string Name { get; }
        public List<DynamicValue> Values { get; } = new();

        public ChunkAttribute(string name)
        {
            Name = name;
        }
    }
}