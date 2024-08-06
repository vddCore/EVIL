namespace EVIL.Ceres.ExecutionEngine.Concurrency;

public sealed class FiberException : VirtualMachineException
{
    public FiberException(string message)
        : base(message)
    {
    }
}