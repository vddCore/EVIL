namespace EVIL.Lexical;

using System;

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