namespace EVIL.Ceres.ExecutionEngine.Collections;

using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public interface IDynamicValueCollection : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
{
    DynamicValue this[DynamicValue index] { get; set; }
}