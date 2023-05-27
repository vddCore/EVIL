using System;
using System.IO;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Modules;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class CoreModuleTest : ModuleTest<CoreModule>
    {
        [Test]
        public void PrintTest()
        {
            var src = "fn test() -> core.print(\"hello, world\");";
            var originalOut = Console.Out;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms) { AutoFlush = true })
                {
                    Console.SetOut(sw);
                    Assert.That(RunEvilCode(src), Is.EqualTo((DynamicValue)12));
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(ms, leaveOpen: true))
                    {
                        Assert.That(sr.ReadToEnd(), Is.EqualTo("hello, world"));
                    }
                }
            }

            Console.SetOut(originalOut);
        }
    }
}