namespace EVIL.Ceres.ExecutionEngine.Marshaling;

using EVIL.Ceres.ExecutionEngine.TypeSystem;

public interface IWriteableObject
{
    DynamicValue this[DynamicValue key] { set; }
    void Set(DynamicValue key, DynamicValue value);
}