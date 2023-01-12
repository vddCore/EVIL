using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class InvalidTypeUsageException : TypeSystemException
    {
        public InvalidTypeUsageException(DynamicValueType actual, DynamicValueType requested)
            : base($"Attempt to use a {actual.Alias()} as a {requested.Alias()}.")
        {
        }
    }
}