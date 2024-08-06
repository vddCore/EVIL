namespace EVIL.Ceres.ExecutionEngine;

public class DivisionByZeroException : RecoverableVirtualMachineException
{
    internal DivisionByZeroException() 
        : base("Attempt to divide by zero.")
    {
    }
}