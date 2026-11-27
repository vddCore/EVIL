namespace EVIL.Assessment.Tests.VM.Language;

using System;
using EVIL.Core.Grammar;
using EVIL.Core.Lexical;
using EVIL.VM.TranslationEngine;

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