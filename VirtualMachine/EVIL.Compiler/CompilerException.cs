using System;

namespace EVIL.Compiler
{
    public class CompilerException : Exception
    {
        public CompilerException(string message) 
            : base(message)
        {
        }
    }
}