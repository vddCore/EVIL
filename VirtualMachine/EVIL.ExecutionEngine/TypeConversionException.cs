using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class TypeConversionException : TypeSystemException
    {
        public TypeConversionException(DynamicValueType source, DynamicValueType target)
            : base($"Cannot convert a {source.Alias()} to a {target.Alias()}.")
        {
        }
    }
}