namespace EVIL.VirtualMachine.ExecutionEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public class UserUnhandledExceptionException : VirtualMachineException
{
    public DynamicValue EvilExceptionObject { get; }
    public StackFrame[] EvilStackTrace { get; }

    internal UserUnhandledExceptionException(string message, DynamicValue evilExceptionObject, StackFrame[] evilStackTrace)
        : base(message)
    {
        EvilExceptionObject = evilExceptionObject;
        EvilStackTrace = evilStackTrace;
    }
}