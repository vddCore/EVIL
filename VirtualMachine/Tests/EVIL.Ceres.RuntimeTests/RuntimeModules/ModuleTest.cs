namespace EVIL.Ceres.RuntimeTests.RuntimeModules;

using EVIL.Ceres.Runtime;

public abstract class ModuleTest<T> : TestBase
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