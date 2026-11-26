namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Debugging;

using EVIL.VirtualMachine.ExecutionEngine.Concurrency;

public delegate void NativeFunctionInvokeHandler(Fiber fiber, NativeFunction nativeFunction);