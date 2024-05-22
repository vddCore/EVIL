using System;
using EVIL.Ceres.ExecutionEngine.Concurrency;

namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public delegate void FiberCrashHandler(Fiber fiber, Exception exception);
}