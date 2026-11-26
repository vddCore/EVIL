namespace EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public sealed class UnsupportedDynamicValueOperationException : RecoverableVirtualMachineException
{
    internal UnsupportedDynamicValueOperationException(string message) 
        : base(message)
    {
    }
}