using System;

namespace EVIL.Execution
{
    public class RuntimeException : Exception
    {
        public int? Line { get; }

        public RuntimeException(string message, int? line) : base(message)
        {
            Line = line;
        }
    }
}
