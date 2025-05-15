namespace EVIL.evil;

using System;

public class RuntimeModuleLoadException(
    string message,
    Exception innerException,
    string filePath
) : Exception(message, innerException)
{
    public string FilePath { get; } = filePath;
}