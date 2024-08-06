namespace EVIL.Ceres.TranslationEngine;

using System;
using EVIL.Ceres.TranslationEngine.Diagnostics;

public class CompilerException : Exception
{
    public CompilerLog Log { get; }

    public CompilerException(CompilerLog log)
        : base("A fatal compiler compiler error occurred.")
    {
        Log = log;
    }
        
    public CompilerException(CompilerLog log, Exception? innerException)
        : base("A fatal compiler compiler error occurred.", innerException)
    {
        Log = log;
    }
}