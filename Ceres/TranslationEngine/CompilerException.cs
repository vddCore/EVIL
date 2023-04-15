using System;

namespace Ceres.TranslationEngine
{
    public class CompilerException : Exception
    {
        public CompilerException(string message)
            : base(message)
        {
        }
    }
}