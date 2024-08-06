namespace EVIL.Ceres.ExecutionEngine.Marshaling;

using EVIL.Ceres.ExecutionEngine.TypeSystem;

public interface IDynamicValueProvider
{
    DynamicValue ToDynamicValue();
}