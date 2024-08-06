namespace EVIL.Ceres.LanguageTests;

using System;
using EVIL.Ceres.TranslationEngine;
using EVIL.Grammar;
using EVIL.Lexical;

public class TestBuildPhaseException : Exception
{
    public TestBuildPhaseException(string message, LexerException lexerException)
        : base(message, lexerException)
    {
    }

    public TestBuildPhaseException(string message, ParserException parserException)
        : base(message, parserException)
    {
    }

    public TestBuildPhaseException(string message, CompilerException compilerException)
        : base(message, compilerException)
    {
    }
}