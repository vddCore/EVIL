using EVIL.ExecutionEngine;

namespace EVIL.RT
{
    public class EvilRuntimeException : VirtualMachineException
    {
        public EvilRuntimeException(string message) 
            : base(message)
        {
        }
    }
}