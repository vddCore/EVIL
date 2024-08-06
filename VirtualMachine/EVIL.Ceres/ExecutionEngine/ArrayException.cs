namespace EVIL.Ceres.ExecutionEngine;

using System;

public class ArrayException : RecoverableVirtualMachineException
{
    internal ArrayException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    internal ArrayException(string message) : base(message)
    {
    }
}