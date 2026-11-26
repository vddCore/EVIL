namespace EVIL.VirtualMachine.Runtime;

using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public class UserFailException : EvilRuntimeException
{
    public Fiber Fiber { get; }
    public DynamicValue[] Arguments { get; }

    public UserFailException(string message, Fiber fiber, DynamicValue[] arguments) 
        : base(message)
    {
        Fiber = fiber;
        Arguments = arguments;
    }
}