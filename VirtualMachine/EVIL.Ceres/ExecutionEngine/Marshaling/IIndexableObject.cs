namespace EVIL.Ceres.ExecutionEngine.Marshaling;

using EVIL.Ceres.ExecutionEngine.TypeSystem;

public interface IIndexableObject
{
    DynamicValue this[DynamicValue key] { get; }
    DynamicValue Index(DynamicValue key);
}