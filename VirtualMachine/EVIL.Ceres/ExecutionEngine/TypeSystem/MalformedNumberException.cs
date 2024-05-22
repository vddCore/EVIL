using System;

namespace EVIL.Ceres.ExecutionEngine.TypeSystem
{
    public class MalformedNumberException : VirtualMachineException
    {
        internal MalformedNumberException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}