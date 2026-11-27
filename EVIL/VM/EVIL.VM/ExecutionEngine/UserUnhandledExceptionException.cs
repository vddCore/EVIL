namespace EVIL.VM.ExecutionEngine;

using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.TypeSystem;

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