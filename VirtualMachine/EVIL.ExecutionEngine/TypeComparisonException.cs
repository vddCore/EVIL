using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class TypeComparisonException : TypeSystemException
    {
        public TypeComparisonException(DynamicValue a, DynamicValue b)
            : base($"Cannot compare a {a.Type.Alias()} with a {b.Type.Alias()}.")
        {
        }
    }
}