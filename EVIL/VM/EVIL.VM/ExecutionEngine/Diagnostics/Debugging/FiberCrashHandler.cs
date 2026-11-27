namespace EVIL.VM.ExecutionEngine.Diagnostics.Debugging;

using System;
using EVIL.VM.ExecutionEngine.Concurrency;

public delegate void FiberCrashHandler(Fiber fiber, Exception exception);