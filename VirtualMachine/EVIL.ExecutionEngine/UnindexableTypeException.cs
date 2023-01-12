using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class UnindexableTypeException : VirtualMachineException
    {
        public UnindexableTypeException(DynamicValueType type) 
            : base($"Attempt to index an unindexable type {type}.")
        {
        }
    }
}