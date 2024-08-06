namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public delegate DynamicValue NativeFunction(Fiber context, params DynamicValue[] args);