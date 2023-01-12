using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class NonInvokableValueException : VirtualMachineException
    {
        public DynamicValue Value { get; }
        
        public NonInvokableValueException(DynamicValue value)
            : base($"Unable to invoke a non-invokable value of type '{value.Type}'.")
        {
            Value = value;
        }
    }
}