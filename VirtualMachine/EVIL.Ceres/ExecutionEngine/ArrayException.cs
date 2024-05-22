using System;

namespace EVIL.Ceres.ExecutionEngine
{
    public class ArrayException : VirtualMachineException
    {
        internal ArrayException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        internal ArrayException(string message) : base(message)
        {
        }
    }
}