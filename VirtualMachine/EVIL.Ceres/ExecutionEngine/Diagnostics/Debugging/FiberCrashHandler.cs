namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

using System;
using EVIL.Ceres.ExecutionEngine.Concurrency;

public delegate void FiberCrashHandler(Fiber fiber, Exception exception);