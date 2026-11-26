namespace EVIL.Core.Grammar;

using System;
using EVIL.Core.Diagnostics;

public class ParserException : Exception
{
    public int Line { get; }
    public int Column { get; }
    public SourceLocation Location { get; }

    public ParserException(string message, (int line, int col) location)
        : base(message)
    {
        (Line, Column) = location;
        Location = new SourceLocation(null, location.line, location.col);
    }

    public ParserException(string message, SourceLocation location)
        : base(message)
    {
        Line = location.Line;
        Column = location.Column;
        Location = location;
    }
}
