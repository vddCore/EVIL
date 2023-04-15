using System;
using Ceres.TranslationEngine;
using EVIL.Grammar;
using EVIL.Lexical;

namespace Insitor
{
    public class TestBuildPhaseException : Exception
    {
        public LexerException? LexerException { get; }
        public ParserException? ParserException { get; }
        public CompilerException? CompilerException { get; }

        public TestBuildPhaseException(string message, LexerException lexerException)
            : base(message)
        {
            LexerException = lexerException;
        }

        public TestBuildPhaseException(string message, ParserException parserException)
            : base(message)
        {
            ParserException = parserException;
        }

        public TestBuildPhaseException(string message, CompilerException compilerException)
            : base(message)
        {
            CompilerException = compilerException;
        }
    }
}