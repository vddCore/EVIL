namespace EVIL.VM.ExecutionEngine.Diagnostics;

using System.Collections.Generic;
using EVIL.VM.ExecutionEngine.TypeSystem;

public class ClosureContext
{
    public string EnclosedFunctionName { get; }
    public Dictionary<int, DynamicValue> Values { get; } = new();

    public ClosureContext(string enclosedFunctionName)
    {
        EnclosedFunctionName = enclosedFunctionName;
    }
}