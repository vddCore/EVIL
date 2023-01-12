using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;

namespace EVIL.ExecutionEngine.Interop
{
    public delegate DynamicValue ClrFunction(ExecutionContext ctx, params DynamicValue[] arguments);
}