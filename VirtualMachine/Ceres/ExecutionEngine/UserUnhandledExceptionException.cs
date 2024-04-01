using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine
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