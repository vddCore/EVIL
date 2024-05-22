namespace EVIL.Ceres.ExecutionEngine
{
    public class ChunkInvocationException : VirtualMachineException
    {
        internal ChunkInvocationException(string message)
            : base(message)
        {
        }
    }
}