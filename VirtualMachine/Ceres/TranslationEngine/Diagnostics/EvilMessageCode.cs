namespace Ceres.TranslationEngine.Diagnostics
{
    public static class EvilMessageCode
    {
        public const int DuplicateSymbolInScope = 0001;
        public const int AttemptToWriteReadOnlyLocal = 0002;
        public const int IllegalIncrementationTarget = 0003;
        public const int IllegalDecrementationTarget = 0004;
        public const int IllegalAssignmentTarget = 0005;
        public const int LexerError = 0006;
        public const int ParserError = 0007;
        public const int IncludeFoundButNoIncludeProcessorsPresent = 0008;
        public const int IncludedFileRedefinedExistingChunk = 0009;
        public const int IncludeProcessorThrew = 0010;
        public const int FnStatementRedefinedExistingChunk = 0011;
        public const int SelfUsedInSelfUnawareFunction = 0012;
        public const int TooManyInitializersForConstSizeArray = 0013;
    }
}