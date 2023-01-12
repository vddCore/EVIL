using System;
using EVIL.ExecutionEngine.Diagnostics;

namespace EVIL.ExecutionEngine
{
    public class VirtualMachineException : Exception
    {
        public ExecutionContext ExecutionContext { get; }

        public VirtualMachineException(ExecutionContext ctx, string message) 
            : base(message)
        {
            ExecutionContext = ctx;
        }

        public VirtualMachineException(ExecutionContext ctx, string message, Exception innerException) 
            : base(message, innerException)
        {
            ExecutionContext = ctx;
        }
    }
}