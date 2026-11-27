namespace EVIL.VM.ExecutionEngine.Collections;

using EVIL.VM.ExecutionEngine.TypeSystem;

public interface IWriteableObject
{
    DynamicValue this[DynamicValue key] { set; }
    void Set(DynamicValue key, DynamicValue value);
}