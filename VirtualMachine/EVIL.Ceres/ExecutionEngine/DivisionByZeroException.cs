namespace EVIL.Ceres.ExecutionEngine
{
    public class DivisionByZeroException : VirtualMachineException
    {
        internal DivisionByZeroException() 
            : base("Attempt to divide by zero.")
        {
        }
    }
}