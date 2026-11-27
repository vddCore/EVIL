namespace EVIL.VM.ExecutionEngine.Diagnostics;

using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.TypeSystem;

public delegate DynamicValue NativeFunction(Fiber context, params DynamicValue[] args);