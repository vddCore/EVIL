using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class NumberFormatException : VirtualMachineException
    {
        public NumberFormatException(DynamicValue sourceValue) 
            : base($"'{sourceValue.String} is not a valid 64-bit floating point number.")
        {
        }
    }
}