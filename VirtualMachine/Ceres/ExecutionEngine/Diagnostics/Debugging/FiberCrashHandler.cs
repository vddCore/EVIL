using System;
using Ceres.ExecutionEngine.Concurrency;

namespace Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public delegate void FiberCrashHandler(Fiber fiber, Exception exception);
}