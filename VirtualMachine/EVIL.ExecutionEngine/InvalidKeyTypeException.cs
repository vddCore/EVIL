using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class InvalidKeyTypeException : TypeSystemException
    {
        public InvalidKeyTypeException(DynamicValueType keyType, DynamicValueType collectionType) 
            : base($"A {keyType.Alias()} cannot be used as a key to index a {collectionType.Alias()}.")
        {
        }
    }
}