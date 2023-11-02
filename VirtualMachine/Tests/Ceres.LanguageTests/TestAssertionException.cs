using System;

namespace Ceres.LanguageTests
{
    public class TestAssertionException : Exception
    {
        public TestAssertionException(string message)
            : base(message)
        {
        }
    }
}