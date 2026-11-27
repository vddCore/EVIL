namespace EVIL.VM.Runtime;

using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.TypeSystem;

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