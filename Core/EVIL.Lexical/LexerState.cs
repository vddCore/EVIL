namespace EVIL.Lexical
{
    public class LexerState
    {
        public Token PreviousToken { get; internal set; }
        public Token CurrentToken { get; internal set; }
        public char Character { get; internal set; }

        public int Pointer { get; internal set; }
        public int Column { get; internal set; }
        public int Line { get; internal set; }

        internal void Reset()
        {
            PreviousToken = null;
            CurrentToken = null;
            Character = (char)0;
            Pointer = 0;
            Column = 1;
            Line = 1;
        }
    }
}