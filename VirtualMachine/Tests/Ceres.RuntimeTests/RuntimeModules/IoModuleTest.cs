using System;
using System.IO;
using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

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
                using (var sw = new StreamWriter(ms))
                {
                    sw.AutoFlush = true;
                    Console.SetOut(sw);
                    
                    EvilTestResult(src).ShouldBe(12);
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(ms, leaveOpen: true))
                    {
                        sr.ReadToEnd().ShouldBe("hello, world");
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
                using (var sw = new StreamWriter(ms))
                    {
                        sw.AutoFlush = true;
                    Console.SetOut(sw);
                    
                    EvilTestResult(src).ShouldBe("hello, world".Length + Environment.NewLine.Length);
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(ms, leaveOpen: true))
                    {
                        sr.ReadToEnd().ShouldBe($"hello, world{Environment.NewLine}");
                    }
                }
            }

            Console.SetOut(originalOut);
        }
    }
}