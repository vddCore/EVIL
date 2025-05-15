namespace EVIL.Lexical;

public record LexerState
{
    public Token PreviousToken { get; internal set; } = Token.Empty;
    public Token CurrentToken { get; internal set; } = Token.Empty;

    public char Character { get; internal set; }

    public int Pointer { get; internal set; }
    public int Column { get; internal set; }
    public int Line { get; internal set; }

    internal void Reset()
    {
        PreviousToken = Token.Empty;
        CurrentToken = Token.Empty;
        Character = (char)0;
        Pointer = 0;
        Column = 1;
        Line = 1;
    }
}