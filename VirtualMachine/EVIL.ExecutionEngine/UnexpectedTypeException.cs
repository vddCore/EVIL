using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class UnexpectedTypeException : VirtualMachineException
    {
        public UnexpectedTypeException(DynamicValueType actual)
            : base($"Type '{actual}' was unexpected at this time.")
        {
        }
        
        public UnexpectedTypeException(DynamicValueType actual, DynamicValueType expected) 
            : base($"Found type {actual} when {expected} was expected.")
        {
        }
    }
}