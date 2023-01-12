using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class UnmeasurableTypeException : TypeSystemException
    {
        public UnmeasurableTypeException(DynamicValueType type) 
            : base($"Cannot compute the length of a {type.Alias()}.")
        {
        }
    }
}