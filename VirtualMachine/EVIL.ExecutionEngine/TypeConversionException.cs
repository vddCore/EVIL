using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class TypeConversionException : TypeSystemException
    {
        public TypeConversionException(DynamicValueType source, DynamicValueType target)
            : base($"Cannot convert a value of type {source} to a {target}.")
        {
        }
    }
}