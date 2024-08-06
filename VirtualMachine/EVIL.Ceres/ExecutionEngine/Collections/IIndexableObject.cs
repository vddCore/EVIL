namespace EVIL.Ceres.ExecutionEngine.Collections;

using EVIL.Ceres.ExecutionEngine.TypeSystem;

public interface IIndexableObject
{
    DynamicValue this[DynamicValue key] { get; }
    DynamicValue Index(DynamicValue key);
}