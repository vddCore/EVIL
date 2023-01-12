using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class InvalidKeyTypeException : TypeSystemException
    {
        public InvalidKeyTypeException(DynamicValueType indexedType, DynamicValueType indexingType) 
            : base($"Type {indexingType} cannot be used as a key to index {indexedType}.")
        {
        }
    }
}