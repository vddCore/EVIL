using System;
using System.IO;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime;
using Ceres.TranslationEngine;
using EVIL.Grammar.Parsing;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class CoreModule
    {
        private CeresVM _vm;
        private EvilRuntime _evilRuntime;

        private Parser _parser;
        private Compiler _compiler;

        [SetUp]
        public void Setup()
        {
            _parser = new Parser();
            _compiler = new Compiler();

            _vm = new CeresVM();

            _evilRuntime = new EvilRuntime(_vm);
            _evilRuntime.RegisterModule<Runtime.Modules.CoreModule>();

            _vm.Start();
        }

        [TearDown]
        public void Teardown()
        {
            _vm.Dispose();
            _evilRuntime = null;
            _parser = null;
            _compiler = null;
        }

        [Test]
        public void PrintTest()
        {
            var src = "fn test() -> core.print(\"hello, world\");";
            var prog = _parser.Parse(src);
            var script = _compiler.Compile(prog);
            var originalOut = Console.Out;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms) { AutoFlush = true })
                {
                    Console.SetOut(sw);

                    _vm.MainFiber.ScheduleAsync(script["test"]!)
                        .GetAwaiter()
                        .GetResult();

                    var value = _vm.MainFiber.PopValue();

                    Assert.That(value, Is.EqualTo((DynamicValue)12));
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