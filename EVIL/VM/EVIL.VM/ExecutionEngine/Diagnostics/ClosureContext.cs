namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

using System.Collections.Generic;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public class ClosureContext
{
    public string EnclosedFunctionName { get; }
    public Dictionary<int, DynamicValue> Values { get; } = new();

    public ClosureContext(string enclosedFunctionName)
    {
        EnclosedFunctionName = enclosedFunctionName;
    }
}