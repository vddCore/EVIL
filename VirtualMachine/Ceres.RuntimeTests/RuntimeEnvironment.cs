using Ceres.ExecutionEngine;
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
    }
}