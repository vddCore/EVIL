using System;

namespace EVIL.Grammar
{
    public class ParserException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public ParserException(string message) : base(message) { }

        public ParserException(string message, (int line, int col) location)
            : base(message)
        {
            (Line, Column) = location;
        }
    }
}
