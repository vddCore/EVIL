namespace EVIL.evil;

using System;

public class RuntimeModuleLoadException : Exception
{
    public string FilePath { get; }

    public RuntimeModuleLoadException(string message, Exception innerException, string filePath) 
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}