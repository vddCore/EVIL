using System;

namespace Ceres.ExecutionEngine.Collections.Serialization
{
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
}