namespace Ceres.TranslationEngine.Diagnostics
{
    public static class EvilMessageCode
    {
        public const int DuplicateSymbolInScope = 1;
        public const int AttemptToWriteReadOnlyLocal = 2;
        public const int IllegalIncrementationTarget = 3;
        public const int IllegalDecrementationTarget = 4;
        public const int IllegalAssignmentTarget = 5;
        public const int LexerError = 6;
        public const int ParserError = 7;
    }
}