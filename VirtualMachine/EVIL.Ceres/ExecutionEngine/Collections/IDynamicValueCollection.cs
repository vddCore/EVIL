using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.Collections
{
    public interface IDynamicValueCollection : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
    {
        DynamicValue this[DynamicValue index] { get; set; }
    }
}