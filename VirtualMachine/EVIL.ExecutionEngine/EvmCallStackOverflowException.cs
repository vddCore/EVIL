using EVIL.ExecutionEngine.Diagnostics;

namespace EVIL.ExecutionEngine
{
    public class EvmCallStackOverflowException : VirtualMachineException
    {
        public EvmCallStackOverflowException(ExecutionContext ctx)
            : base(ctx, "Stack overflow has occurred. Increase the VM call stack limit or write better code.")
        {
        }
    }
}