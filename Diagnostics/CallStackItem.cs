using System.Collections.Generic;
using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Diagnostics
{
    public class CallStackItem
    {
        public string FunctionName { get; }
        public Dictionary<string, DynValue> Parameters { get; }

        public bool ReturnNow { get; private set; }
        public DynValue ReturnValue { get; set; } = DynValue.Zero;

        public CallStackItem(string functionName)
        {
            FunctionName = functionName;
            Parameters = new Dictionary<string, DynValue>();
        }

        public bool HasParameter(string name)
            => Parameters.ContainsKey(name);

        public void SetParameter(string name, DynValue value)
        {
            if (!Parameters.ContainsKey(name))
                throw new RuntimeException($"There is no parameter called '{name}' in this scope.", null);

            Parameters[name] = value;
        }

        public void Return()
        {
            ReturnNow = true;
        }
    }
}
