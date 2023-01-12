using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class InvalidKeyTypeException : VirtualMachineException
    {
        public InvalidKeyTypeException(DynamicValueType type) 
            : base($"Type {type} cannot be used as a table key.")
        {
        }
    }
}