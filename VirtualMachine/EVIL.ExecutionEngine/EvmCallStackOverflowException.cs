namespace EVIL.ExecutionEngine
{
    public class EvmCallStackOverflowException : VirtualMachineException
    {
        public EvmCallStackOverflowException()
            : base("Stack overflow has occurred. Increase the VM call stack limit or write better code.")
        {
        }
    }
}