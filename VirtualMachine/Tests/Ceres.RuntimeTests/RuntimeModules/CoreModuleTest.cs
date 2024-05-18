using System;
using Ceres.Runtime;
using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
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
}