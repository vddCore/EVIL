using System;

namespace EVIL.evil
{
    public class RuntimeModuleLoadException : Exception
    {
        public string FilePath { get; }

        public RuntimeModuleLoadException(string message, Exception innerException, string filePath) 
            : base(message, innerException)
        {
            FilePath = filePath;
        }
    }
}