using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Conversions
    {
        private EVM _evm;
        
        [SetUp]
        public void SetUp()
        {
            _evm = new EVM();
        }
        
        [Test]
        public void NumberToString()
        {
            var val = _evm.Evaluate("ret @21.37;");
            
            Assert.AreEqual(DynamicValueType.String, val.Type);
            Assert.AreEqual("21.37", val.String);
        }

        [Test]
        public void StringToNumber()
        {
            var val = _evm.Evaluate("ret $\"21.37\";");
            
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21.37, val.Number);
        }
    }
}