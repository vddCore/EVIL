namespace Ceres.ExecutionEngine.TypeSystem
{
    public sealed class UnsupportedDynamicValueOperationException : VirtualMachineException
    {
        internal UnsupportedDynamicValueOperationException(string message) 
            : base(message)
        {
        }
    }
}