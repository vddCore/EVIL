namespace EVIL.Assessment.Tests.VM.Language;

using System;

public class TestAssertionException : Exception
{
    public TestAssertionException(string message)
        : base(message)
    {
    }
}