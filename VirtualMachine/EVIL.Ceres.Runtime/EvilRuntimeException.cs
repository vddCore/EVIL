namespace EVIL.Ceres.Runtime;

using System;

public class EvilRuntimeException : Exception
{
    public EvilRuntimeException(string message)
        : base(message)
    {
    }
        
    public EvilRuntimeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}