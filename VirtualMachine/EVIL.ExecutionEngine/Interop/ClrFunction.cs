using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine.Interop
{
    public delegate DynamicValue ClrFunction(EVM evm, params DynamicValue[] arguments);
}