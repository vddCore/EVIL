using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public delegate DynamicValue NativeFunction(Fiber context, params DynamicValue[] args);
}