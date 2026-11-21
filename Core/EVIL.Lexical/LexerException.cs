namespace EVIL.Lexical;

using System;
using EVIL.CommonTypes.Diagnostics;

public class LexerException : Exception
{
    public int Line { get; }
    public int Column { get; }
    public SourceLocation Location { get; }

    public LexerException(string message, int line, int column)
        : base(message)
    {
        Line = line;
        Column = column;
        Location = new SourceLocation(null, line, column);
    }

    public LexerException(string message, SourceLocation location)
        : base(message)
    {
        Line = location.Line;
        Column = location.Column;
        Location = location;
    }
}
