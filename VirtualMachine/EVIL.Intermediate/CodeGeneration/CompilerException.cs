using System;

namespace EVIL.Intermediate.CodeGeneration
{
    public class CompilerException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public CompilerException(string message, int line, int column) 
            : this(message, null, line, column)
        {
        }

        public CompilerException(string message, Exception innerException, int line, int column)
            : base(message, innerException)
        {
            Line = line;
            Column = column;
        }
    }
}