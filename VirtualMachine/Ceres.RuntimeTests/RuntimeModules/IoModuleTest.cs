using System;
using System.IO;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Modules;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class IoModuleTest : ModuleTest<IoModule>
    {
        [Test]
        public void Print()
        {
            var src = "fn test() -> io.print(\"hello, world\");";
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

        [Test]
        public void PrintLine()
        {
            var src = "fn test() -> io.println(\"hello, world\");";
            var originalOut = Console.Out;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms) { AutoFlush = true })
                {
                    Console.SetOut(sw);
                    Assert.That(RunEvilCode(src), Is.EqualTo((DynamicValue)14));
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(ms, leaveOpen: true))
                    {
                        Assert.That(sr.ReadToEnd(), Is.EqualTo($"hello, world{Environment.NewLine}"));
                    }
                }
            }

            Console.SetOut(originalOut);
        }
    }
}