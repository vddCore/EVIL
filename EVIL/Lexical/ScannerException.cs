using System;

namespace EVIL.Lexical
{
    public class ScannerException : Exception
    {
        public int Column { get; }
        public int Line { get; }

        public ScannerException(string message, int column, int line) : base(message)
        {
            Column = column;
            Line = line;
        }
    }
}
