using System;

namespace Ceres.TranslationEngine
{
    public class CompilerException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public CompilerException(int line, int column, string message)
            : base(message)
        {
            Line = line;
            Column = column;
        }
        
        public CompilerException(int line, int column, string message, Exception innerException)
            : base(message, innerException)
        {
            Line = line;
            Column = column;
        }
    }
}