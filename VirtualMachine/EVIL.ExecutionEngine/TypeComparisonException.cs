using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class TypeComparisonException : VirtualMachineException
    {
        public TypeComparisonException(DynamicValue a, DynamicValue b)
            : base($"Cannot compare types {a.Type} with {b.Type}.")
        {
        }
    }
}