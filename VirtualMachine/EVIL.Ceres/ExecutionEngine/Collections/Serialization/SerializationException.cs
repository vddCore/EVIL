namespace EVIL.Ceres.ExecutionEngine.Collections.Serialization;

using System;

public class SerializationException : VirtualMachineException
{
    internal SerializationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    internal SerializationException(string message) 
        : base(message)
    {
    }
}