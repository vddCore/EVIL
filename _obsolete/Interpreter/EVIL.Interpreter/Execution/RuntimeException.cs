using System;
using System.Collections.Generic;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public class RuntimeException : Exception
    {
        public int? Line { get; }
        public List<StackFrame> EvilStackTrace { get; private set; }

        public RuntimeException(string message, Interpreter interpreter, int? line) : base(message)
        {
            EvilStackTrace = interpreter.StackTrace();
            Line = line;
        }

        public RuntimeException(string message, Interpreter interpreter, int? line, Exception innerException)
            : base(message, innerException)
        {
            EvilStackTrace = interpreter.StackTrace();
            Line = line;
        }
    }
}