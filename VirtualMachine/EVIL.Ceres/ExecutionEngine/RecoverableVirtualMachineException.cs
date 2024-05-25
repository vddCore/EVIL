using System;

namespace EVIL.Ceres.ExecutionEngine
{
    public class RecoverableVirtualMachineException : VirtualMachineException
    {
        internal RecoverableVirtualMachineException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        internal RecoverableVirtualMachineException(string message)
            : base(message)
        {
        }
    }
}