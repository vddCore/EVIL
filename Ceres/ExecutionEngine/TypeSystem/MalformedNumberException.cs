using System;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public class MalformedNumberException : VirtualMachineException
    {
        internal MalformedNumberException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}