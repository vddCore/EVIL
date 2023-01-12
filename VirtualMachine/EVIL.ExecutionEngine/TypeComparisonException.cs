using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class TypeComparisonException : TypeSystemException
    {
        public TypeComparisonException(DynamicValue a, DynamicValue b)
            : base($"Cannot compare types {a.Type} with {b.Type}.")
        {
        }
    }
}