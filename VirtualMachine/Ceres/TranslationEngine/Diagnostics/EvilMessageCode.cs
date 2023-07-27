﻿namespace Ceres.TranslationEngine.Diagnostics
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
    }
}