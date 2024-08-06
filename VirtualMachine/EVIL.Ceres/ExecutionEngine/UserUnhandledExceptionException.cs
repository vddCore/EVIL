namespace EVIL.Ceres.ExecutionEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

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