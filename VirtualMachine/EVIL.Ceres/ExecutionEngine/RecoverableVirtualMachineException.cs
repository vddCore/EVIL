namespace EVIL.Ceres.ExecutionEngine;

using System;

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