namespace EVIL.VM.ExecutionEngine.Diagnostics;

using System.Collections.Generic;
using EVIL.VM.ExecutionEngine.TypeSystem;

public sealed class SwitchTable
{
    public int DefaultIp { get; }
    public IReadOnlyDictionary<DynamicValue, int> Cases { get; }

    public SwitchTable(int defaultIp, Dictionary<DynamicValue, int> cases)
    {
        DefaultIp = defaultIp;
        Cases = cases;
    }
}
