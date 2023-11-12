using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime
{
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
}