namespace EVIL.Ceres.LanguageTests;

using System;

public class TestAssertionException : Exception
{
    public TestAssertionException(string message)
        : base(message)
    {
    }
}