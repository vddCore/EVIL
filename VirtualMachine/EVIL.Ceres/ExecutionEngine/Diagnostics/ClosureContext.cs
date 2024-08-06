namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public class ClosureContext
{
    public string EnclosedFunctionName { get; }
    public Dictionary<int, DynamicValue> Values { get; } = new();

    public ClosureContext(string enclosedFunctionName)
    {
        EnclosedFunctionName = enclosedFunctionName;
    }
}