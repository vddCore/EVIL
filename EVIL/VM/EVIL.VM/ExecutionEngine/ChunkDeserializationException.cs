namespace EVIL.VM.ExecutionEngine;

public class ChunkDeserializationException : VirtualMachineException
{
    internal ChunkDeserializationException(string message) 
        : base(message)
    {
    }
}