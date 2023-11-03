using Ceres.ExecutionEngine.Concurrency;

namespace Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public delegate void NativeFunctionInvokeHandler(Fiber fiber, NativeFunction nativeFunction);
}