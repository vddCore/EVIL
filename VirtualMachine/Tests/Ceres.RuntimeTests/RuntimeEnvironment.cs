using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.Runtime;
using NUnit.Framework;

namespace Ceres.RuntimeTests
{
    public class RuntimeEnvironment
    {
        private CeresVM _vm;
        private EvilRuntime _evilRuntime;

        [SetUp]
        public void Setup()
        {
            _vm = new CeresVM();
            _evilRuntime = new EvilRuntime(_vm);
            _vm.Start();
        }

        [TearDown]
        public void Teardown()
        {
            _vm.Dispose();

            _evilRuntime = null;
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
}