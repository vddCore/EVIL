using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.Diagnostics
{
    public class ClosureContext
    {
        public string EnclosedFunctionName { get; }
        public Dictionary<int, DynamicValue> Values { get; } = new();

        public ClosureContext(string enclosedFunctionName)
        {
            EnclosedFunctionName = enclosedFunctionName;
        }
    }
}