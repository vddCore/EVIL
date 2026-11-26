namespace EVIL.Assessment.Tests.VM.Runtime.ModuleTests;

using EVIL.VirtualMachine.Runtime;
using EVIL.Assessment.Tests.Base;

public abstract class ModuleTest<T> : EvilTest
    where T : RuntimeModule, new()
{
    private EvilRuntime _evilRuntime = null!;

    protected override void OnBeforeVirtualMachineInitialized()
    {
        _evilRuntime = new EvilRuntime(VM);
        _evilRuntime.RegisterModule<T>(out _);
    }

    public override void Teardown()
    {
        base.Teardown();
        _evilRuntime = null!;
    }
}