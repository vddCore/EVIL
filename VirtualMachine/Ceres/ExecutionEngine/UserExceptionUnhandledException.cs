using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine
{
    public class UserExceptionUnhandledException : VirtualMachineException
    {
        public DynamicValue UserExceptionObject { get; }
        public StackFrame[] EvilStackTrace { get; }

        internal UserExceptionUnhandledException(string message, DynamicValue userExceptionObject, StackFrame[] evilStackTrace)
            : base(message)
        {
            UserExceptionObject = userExceptionObject;
            EvilStackTrace = evilStackTrace;
        }
    }
}