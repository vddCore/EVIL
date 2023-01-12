using EVIL.ExecutionEngine.Diagnostics;

namespace EVIL.ExecutionEngine
{
    public class AddressOutOfBoundsException : VirtualMachineException
    {
        public StackFrame Frame { get; }

        public AddressOutOfBoundsException(StackFrame frame, int addr)
            : base($"Attempted execution of instruction at address {addr:X8} which is out of bounds.")
        {
            Frame = frame;
        }
    }
}