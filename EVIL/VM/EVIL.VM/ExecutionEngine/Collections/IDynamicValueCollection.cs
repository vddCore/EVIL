namespace EVIL.VirtualMachine.ExecutionEngine.Collections;

using System.Collections.Generic;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public interface IDynamicValueCollection : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
{
    DynamicValue this[DynamicValue index] { get; set; }
}