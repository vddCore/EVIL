namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

using EVIL.Ceres.ExecutionEngine.Concurrency;

public delegate void NativeFunctionInvokeHandler(Fiber fiber, NativeFunction nativeFunction);