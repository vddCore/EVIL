namespace EVIL.VirtualMachine.ExecutionEngine.Concurrency;

public sealed class FiberException : VirtualMachineException
{
    public FiberException(string message)
        : base(message)
    {
    }
}