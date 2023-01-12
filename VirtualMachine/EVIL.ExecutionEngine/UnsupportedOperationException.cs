namespace EVIL.ExecutionEngine
{
    public class UnsupportedOperationException : VirtualMachineException
    {
        public UnsupportedOperationException(string message) 
            : base(message)
        {
        }
    }
}