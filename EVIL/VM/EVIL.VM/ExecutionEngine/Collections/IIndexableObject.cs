namespace EVIL.VirtualMachine.ExecutionEngine.Collections;

using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public interface IIndexableObject
{
    DynamicValue this[DynamicValue key] { get; }
    DynamicValue Index(DynamicValue key);
}