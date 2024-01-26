using System.Collections.Generic;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Collections
{
    public interface IDynamicValueCollection : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
    {
        DynamicValue this[DynamicValue index] { get; set; }
    }
}