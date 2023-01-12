using System;

namespace EVIL.Intermediate
{
    public class CompilerException : Exception
    {
        public int Line { get; }

        public CompilerException(string message, int line = -1) 
            : this(message, null, line)
        {
        }

        public CompilerException(string message, Exception innerException, int line = -1)
            : base(message, innerException)
        {
            Line = line;
        }
    }
}