namespace EVIL.Ceres.RuntimeTests.RuntimeModules;

using System;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

public class CoreModuleTest : ModuleTest<CoreModule>
{
    [Test]
    public void CoreFailTerminatesExecution()
    {
        var e = Should.Throw<Exception>(() => EvilTestResult("(fn -> core.fail(':( your computer ran'))();"));
        e.InnerException.ShouldBeOfType<UserFailException>();
        ((UserFailException)e.InnerException).Message.ShouldBe(":( your computer ran");
    }
}