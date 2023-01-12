using System;

namespace EVIL.ExecutionEngine
{
    public class VirtualMachineException : Exception
    {
        public VirtualMachineException(string message) 
            : base(message)
        {
        }

        public VirtualMachineException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}