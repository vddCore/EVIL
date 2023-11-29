using System.Collections.Generic;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
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