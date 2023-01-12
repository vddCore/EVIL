using System;
using System.Collections.Generic;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public class RuntimeException : Exception
    {
        public int? Line { get; }
        public List<StackFrame> EvilStackTrace { get; private set; }

        public RuntimeException(string message, Environment environment, int? line) : base(message)
        {
            EvilStackTrace = environment.StackTrace();
            Line = line;
        }

        public RuntimeException(string message, Environment environment, int? line, Exception innerException)
            : base(message, innerException)
        {
            EvilStackTrace = environment.StackTrace();
            Line = line;
        }
    }
}