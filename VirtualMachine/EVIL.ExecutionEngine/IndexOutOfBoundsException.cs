using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    public class IndexOutOfBoundsException : TypeSystemException
    {
        public IndexOutOfBoundsException(int index, DynamicValueType type) 
            : base($"Index '{index}' is out of bounds for this {type}.")
        {
        }
    }
}