namespace EVIL.Grammar;

using System;

public class ParserException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public ParserException(string message, (int line, int col) location)
        : base(message)
    {
        (Line, Column) = location;
    }
}