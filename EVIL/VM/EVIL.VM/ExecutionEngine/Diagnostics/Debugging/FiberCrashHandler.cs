namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Debugging;

using System;
using EVIL.VirtualMachine.ExecutionEngine.Concurrency;

public delegate void FiberCrashHandler(Fiber fiber, Exception exception);