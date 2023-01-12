using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class GlobalNotFoundException : VirtualMachineException
    {
        public GlobalNotFoundException(DynamicValue constKey) 
            : base($"Attempt to access an undefined variable '{constKey.AsString()}'.")
        {
        }
    }
}