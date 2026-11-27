namespace EVIL.Assessment.Tests.VM.Runtime;

using System;
using System.Threading.Tasks;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.Runtime;
using NUnit.Framework;

public class RuntimeEnvironmentTests
{
    private VirtualMachine _vm;
    private EvilRuntime _evilRuntime;

    [SetUp]
    public void Setup()
    {
        _vm = new VirtualMachine();
        _evilRuntime = new EvilRuntime(_vm);
        _vm.Run();
    }

    [TearDown]
    public void Teardown()
    {
        _vm.Stop();
        _evilRuntime = null!;
    }

    [Test]
    public async Task BuiltInFunctionsRegistered()
    {
        _evilRuntime.RegisterBuiltInModules();
        _evilRuntime.RegisterBuiltInFunctions();

        await _vm.MainFiber.BlockUntilFinishedAsync().WaitAsync(TimeSpan.FromSeconds(5));

        Assert.That(_vm.Global["dofile"].Type, Is.Not.EqualTo(DynamicValueType.Nil));
        Assert.That(_vm.Global["require"].Type, Is.Not.EqualTo(DynamicValueType.Nil));
    }

    [Test]
    public void GlobalNumberRegistered()
    {
        var value = _evilRuntime.Register("testvalue", 21.37);

        Assert.That(
            _vm.Global["testvalue"],
            Is.EqualTo(value)
        );
    }


    [Test]
    public void GlobalStringRegistered()
    {
        var value = _evilRuntime.Register("testvalue", "blah");

        Assert.That(
            _vm.Global["testvalue"],
            Is.EqualTo(value)
        );
    }

    [Test]
    public void GlobalBooleanRegistered()
    {
        var value = _evilRuntime.Register("testvalue", true);

        Assert.That(
            _vm.Global["testvalue"],
            Is.EqualTo(value)
        );
    }

    [Test]
    public void GlobalTableRegistered()
    {
        var t = new Table();
            
        var value = _evilRuntime.Register("testvalue", t);

        Assert.That(
            _vm.Global["testvalue"],
            Is.EqualTo(value)
        );
    }

    [Test]
    public void FullyQualifiedNumberRegistered()
    {
        var value = _evilRuntime.Register("__rt.values.TEST_VALUE", 21.37);

        Assert.That(
            _vm.Global["__rt"].Table!["values"].Table!["TEST_VALUE"],
            Is.EqualTo(value)
        );
    }
}