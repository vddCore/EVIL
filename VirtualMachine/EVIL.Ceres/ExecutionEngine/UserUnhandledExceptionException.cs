using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine
{
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
}