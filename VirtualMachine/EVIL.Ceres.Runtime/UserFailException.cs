using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.Runtime
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