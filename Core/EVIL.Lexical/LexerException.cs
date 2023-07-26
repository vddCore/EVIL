using System;

namespace EVIL.Lexical
{
    public class LexerException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public LexerException(string message, int line, int column) : base(message)
        {
            Line = line;
            Column = column;
        }
    }
}
