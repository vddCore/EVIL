using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine
{
    public class UserExceptionUnhandledException : VirtualMachineException
    {
        public DynamicValue UserExceptionObject { get; }
        public StackFrame[] StackTrace { get; }

        internal UserExceptionUnhandledException(string message, DynamicValue userExceptionObject, StackFrame[] stackTrace)
            : base(message)
        {
            UserExceptionObject = userExceptionObject;
            StackTrace = stackTrace;
        }
    }
}