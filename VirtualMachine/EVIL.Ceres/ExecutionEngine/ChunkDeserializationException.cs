namespace EVIL.Ceres.ExecutionEngine;

public class ChunkDeserializationException : VirtualMachineException
{
    internal ChunkDeserializationException(string message) 
        : base(message)
    {
    }
}