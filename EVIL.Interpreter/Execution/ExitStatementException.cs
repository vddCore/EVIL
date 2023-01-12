using System;
using System.Collections.Generic;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public class ExitStatementException : Exception
    {
        public List<StackFrame> EvilStackTrace { get; private set; }

        public ExitStatementException(List<StackFrame> evilStackTrace)
        {
            EvilStackTrace = evilStackTrace;
        }
    }
}
