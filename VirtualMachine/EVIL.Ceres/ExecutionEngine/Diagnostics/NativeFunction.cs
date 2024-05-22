using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.Diagnostics
{
    public delegate DynamicValue NativeFunction(Fiber context, params DynamicValue[] args);
}