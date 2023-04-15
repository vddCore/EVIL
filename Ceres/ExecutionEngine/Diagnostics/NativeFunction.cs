using System.Threading;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public delegate DynamicValue NativeFunction(ExecutionContext context, params DynamicValue[] args);
}