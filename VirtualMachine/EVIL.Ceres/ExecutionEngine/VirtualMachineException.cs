namespace EVIL.Ceres.ExecutionEngine;

using System;

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