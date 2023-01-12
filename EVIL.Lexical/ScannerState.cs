namespace EVIL.Lexical
{
    public class ScannerState
    {
        public Token PreviousToken { get; internal set; }
        public Token CurrentToken { get; internal set; }
        public char Character { get; internal set; }

        public int Pointer { get; internal set; }
        public int Column { get; internal set; }
        public int Line { get; internal set; }
    }
}