using System.Collections.Generic;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Diagnostics
{
    public class StackFrame
    {
        public string FunctionName { get; }
        public List<string> Parameters { get; } = new();

        public bool ReturnNow { get; private set; }
        public DynValue ReturnValue { get; set; } = DynValue.Zero;

        public int DefinedAtLine { get; set; }
        public int InvokedAtLine { get; set; }

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
