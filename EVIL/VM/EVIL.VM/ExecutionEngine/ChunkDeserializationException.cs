namespace EVIL.VirtualMachine.ExecutionEngine;

public class ChunkDeserializationException : VirtualMachineException
{
    internal ChunkDeserializationException(string message) 
        : base(message)
    {
    }
}