using System;

namespace Ceres.ExecutionEngine
{
    public class VirtualMachineException : Exception
    {
        internal VirtualMachineException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
        
        internal VirtualMachineException(string message) 
            : base(message)
        {
        }
    }
}   