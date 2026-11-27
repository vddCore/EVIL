namespace EVIL.Assessment.Tests.VM.Runtime.ModuleTests;

using System;
using EVIL.VM.Runtime;
using EVIL.VM.Runtime.Modules;
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