namespace EVIL.Ceres.RuntimeTests.RuntimeModules;

using System;
using System.Threading;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.TranslationEngine;
using EVIL.Grammar.Parsing;
using NUnit.Framework;

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