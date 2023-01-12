using System.Collections.Generic;
using EVIL.Interpreter.Execution;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Diagnostics
{
    public class StackFrame
    {
        public string FunctionName { get; }
        public List<string> ParameterNames { get; }

        public bool ReturnNow { get; private set; }
        public DynValue ReturnValue { get; set; } = DynValue.Zero;

        public int DefinedAtLine { get; set; }
        public int InvokedAtLine { get; set; }

        public StackFrame(string functionName, List<string> parameterNames)
        {
            FunctionName = functionName;
            ParameterNames = new List<string>();

            if (parameterNames != null)
            {
                ParameterNames.AddRange(parameterNames);
            }
        }

        public void Return()
        {
            ReturnNow = true;
        }
    }
}
