﻿using System;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime;
using Ceres.TranslationEngine;
using EVIL.Grammar.Parsing;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public abstract class ModuleTest<T> where T : RuntimeModule, new()
    {
        private CeresVM _vm;
        private EvilRuntime _evilRuntime;

        private Parser _parser;
        private Compiler _compiler;

        [SetUp]
        public virtual void Setup()
        {
            _parser = new Parser();
            _compiler = new Compiler();

            _vm = new CeresVM();

            _evilRuntime = new EvilRuntime(_vm);
            _evilRuntime.RegisterModule<T>();

            _vm.Start();
        }

        [TearDown]
        public virtual void Teardown()
        {
            _vm.Dispose();
            _evilRuntime = null;
            _parser = null;
            _compiler = null;
        }

        protected DynamicValue EvilTestResult(string source)
        {
            var script = _compiler.Compile(source, "!module_test_file!");

            foreach (var chunk in script.Chunks)
                _vm.Global[chunk.Name] = chunk;


            _vm.MainFiber.ScheduleAsync(script["test"]!)
                .GetAwaiter()
                .GetResult();

            if (_vm.MainFiber.State == FiberState.Finished)
            {
                return _vm.MainFiber.PopValue();
            }

            throw new Exception("Test failed.");
        }
    }
}