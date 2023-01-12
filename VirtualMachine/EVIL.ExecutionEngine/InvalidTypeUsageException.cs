using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class InvalidTypeUsageException : VirtualMachineException
    {
        public InvalidTypeUsageException(DynamicValueType actual, DynamicValueType requested)
            : base($"Attempted to use a {actual} as a {requested}.")
        {
        }
    }
}