namespace EVIL.Lexical
{
    public class LexerState
    {
        public Token PreviousToken { get; internal set; } = Token.Empty;
        public Token CurrentToken { get; internal set; } = Token.Empty;

        public char Character { get; internal set; }

        public int Pointer { get; internal set; }
        public int Column { get; internal set; }
        public int Line { get; internal set; }

        public LexerState Copy() => new()
        {
            Character = Character,
            Column = Column,
            CurrentToken = CurrentToken,
            Line = Line,
            Pointer = Pointer,
            PreviousToken = PreviousToken
        };

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
}