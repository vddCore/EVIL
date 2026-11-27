namespace EVIL.Core.Grammar;

using System;
using EVIL.Core.Diagnostics;

public class ParserException : Exception
{
    public int Line { get; }
    public int Column { get; }
    public SourceLocation Location { get; }
    public bool IsIncomplete { get; }

    public ParserException(string message, (int line, int col) location, bool isIncomplete = false)
        : base(message)
    {
        (Line, Column) = location;
        Location = new SourceLocation(null, location.line, location.col);
        IsIncomplete = isIncomplete;
    }

    public ParserException(string message, SourceLocation location, bool isIncomplete = false)
        : base(message)
    {
        Line = location.Line;
        Column = location.Column;
        Location = location;
        IsIncomplete = isIncomplete;
    }
}
