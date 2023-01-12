using System.Collections.Generic;
using EVIL.Abstraction;

namespace EVIL.Diagnostics
{
    public class CallStackItem
    {
        public string FunctionName { get; }
        public Dictionary<string, DynValue> ParameterScope { get; }
        public Dictionary<string, DynValue> LocalVariableScope { get; }

        public bool ReturnNow { get; set; }
        public DynValue ReturnValue { get; set; } = DynValue.Zero;

        public CallStackItem(string functionName)
        {
            FunctionName = functionName;

            ParameterScope = new Dictionary<string, DynValue>();
            LocalVariableScope = new Dictionary<string, DynValue>();
        }
    }
}
