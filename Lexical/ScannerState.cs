namespace EVIL.Lexical
{
    public class ScannerState
    {
        public Token PreviousToken { get; set; }
        public Token CurrentToken { get; set; }
        public char Character { get; set; }

        public int Pointer { get; set; }
        public int Column { get; set; }
        public int Line { get; set; }
    }
}