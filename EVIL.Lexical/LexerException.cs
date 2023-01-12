using System;

namespace EVIL.Lexical
{
    public class LexerException : Exception
    {
        public int Column { get; }
        public int Line { get; }

        public LexerException(string message, int column, int line) : base(message)
        {
            Column = column;
            Line = line;
        }
    }
}
