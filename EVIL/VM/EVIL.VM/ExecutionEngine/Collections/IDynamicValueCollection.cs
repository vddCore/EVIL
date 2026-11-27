namespace EVIL.VM.ExecutionEngine.Collections;

using System.Collections.Generic;
using EVIL.VM.ExecutionEngine.TypeSystem;

public interface IDynamicValueCollection : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
{
    DynamicValue this[DynamicValue index] { get; set; }
}