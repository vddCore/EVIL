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
        public void Print()
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
        
        [Test]
        public void PrintLine()
        {
            var src = "fn test() -> core.println(\"hello, world\");";
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

        [Test]
        public void StackTrace()
        {
            var t = RunEvilCode("fn test(a = 1, b = 2, c = 3) -> core.strace(true);").Table!;
            var frame = t[0].Table!;
            
            Assert.That(frame["is_script"].Boolean, Is.EqualTo(true));
            Assert.That(frame["fn_name"].String!, Is.EqualTo("test"));
            Assert.That(frame["def_on_line"].Number, Is.EqualTo(1));
            Assert.That(frame["args"].Table![0].Number, Is.EqualTo(1));
            Assert.That(frame["args"].Table![1].Number, Is.EqualTo(2));
            Assert.That(frame["args"].Table![2].Number, Is.EqualTo(3));
        }
    }
}