using System.Collections.Generic;
using EVIL.Interpreter.Execution;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Diagnostics
{
    public class StackFrame
    {
        public string FunctionName { get; }

        public bool ReturnNow { get; private set; }
        public DynValue ReturnValue { get; set; } = DynValue.Zero;

        public StackFrame(string functionName)
        {
            FunctionName = functionName;
        }

        public void Return()
        {
            ReturnNow = true;
        }
    }
}
