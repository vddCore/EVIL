namespace EVIL.VM.ExecutionEngine;

public class DivisionByZeroException : RecoverableVirtualMachineException
{
    internal DivisionByZeroException() 
        : base("Attempt to divide by zero.")
    {
    }
}