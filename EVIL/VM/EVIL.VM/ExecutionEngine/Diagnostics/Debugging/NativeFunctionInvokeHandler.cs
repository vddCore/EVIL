namespace EVIL.VM.ExecutionEngine.Diagnostics.Debugging;

using EVIL.VM.ExecutionEngine.Concurrency;

public delegate void NativeFunctionInvokeHandler(Fiber fiber, NativeFunction nativeFunction);