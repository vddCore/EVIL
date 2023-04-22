using System;

namespace Ceres.TranslationEngine
{
    public class CompilerException : Exception
    {
        public CompilerException(string message)
            : base(message)
        {
        }
        
        public CompilerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}