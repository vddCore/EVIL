namespace EVIL.VirtualMachine.ExecutionEngine.Collections;

using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public interface IWriteableObject
{
    DynamicValue this[DynamicValue key] { set; }
    void Set(DynamicValue key, DynamicValue value);
}