namespace EVIL.Lexical;

using System;

public class LexerException(string message, int line, int column)
    : Exception(message)
{
    public int Line { get; } = line;
    public int Column { get; } = column;
}