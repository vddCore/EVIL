namespace EVIL.VM.ExecutionEngine.Collections;

using EVIL.VM.ExecutionEngine.TypeSystem;

public interface IIndexableObject
{
    DynamicValue this[DynamicValue key] { get; }
    DynamicValue Index(DynamicValue key);
}