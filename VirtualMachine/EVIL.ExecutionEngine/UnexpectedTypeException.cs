using System;

namespace EVIL.ExecutionEngine
{
    public class UnexpectedTypeException : VirtualMachineException
    {
        public UnexpectedTypeException(string message) 
            : base(message)
        {
        }

        public UnexpectedTypeException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}