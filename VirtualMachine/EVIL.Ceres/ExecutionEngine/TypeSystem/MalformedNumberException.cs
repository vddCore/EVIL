namespace EVIL.Ceres.ExecutionEngine.TypeSystem;

using System;

public class MalformedNumberException : RecoverableVirtualMachineException
{
    internal MalformedNumberException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}