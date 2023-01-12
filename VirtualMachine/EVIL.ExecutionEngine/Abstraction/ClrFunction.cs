namespace EVIL.ExecutionEngine.Abstraction
{
    public delegate DynamicValue ClrFunction(EVM evm, params DynamicValue[] arguments);
}