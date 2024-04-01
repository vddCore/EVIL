using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine
{
    public class UserExceptionUnhandledException : VirtualMachineException
    {
        public DynamicValue UserExceptionObject { get; }

        internal UserExceptionUnhandledException(string message, DynamicValue userExceptionObject)
            : base(message)
        {
            UserExceptionObject = userExceptionObject;
        }
    }
}