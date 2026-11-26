namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

using System.Collections.Generic;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

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
