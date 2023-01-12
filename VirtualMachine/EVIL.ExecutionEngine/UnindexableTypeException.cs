using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class UnindexableTypeException : TypeSystemException
    {
        public UnindexableTypeException(DynamicValueType type) 
            : base($"Attempt to index a {type.Alias()}.")
        {
        }
    }
}