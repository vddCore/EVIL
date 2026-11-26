namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public delegate DynamicValue NativeFunction(Fiber context, params DynamicValue[] args);