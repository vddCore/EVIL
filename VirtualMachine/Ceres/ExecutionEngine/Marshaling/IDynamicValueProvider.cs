using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Marshaling
{
    public interface IDynamicValueProvider
    {
        DynamicValue ToDynamicValue();
    }
}