using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class UnmeasurableTypeException : VirtualMachineException
    {
        public UnmeasurableTypeException(DynamicValueType type) 
            : base($"Cannot compute the length of type {type}.")
        {
        }
    }
}