using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class UnexpectedTypeException : TypeSystemException
    {
        public UnexpectedTypeException(DynamicValueType actual)
            : base($"A {actual.Alias()} was unexpected at this time.")
        {
        }
        
        public UnexpectedTypeException(DynamicValueType actual, DynamicValueType expected) 
            : base($"Found a {actual.Alias()} when a {expected.Alias()} was expected.")
        {
        }
    }
}