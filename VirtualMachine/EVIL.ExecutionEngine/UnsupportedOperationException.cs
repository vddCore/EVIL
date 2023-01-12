namespace EVIL.ExecutionEngine
{
    public class UnsupportedOperationException : TypeSystemException
    {
        public UnsupportedOperationException(string message) 
            : base(message)
        {
        }
    }
}