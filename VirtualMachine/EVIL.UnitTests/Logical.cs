using EVIL.ExecutionEngine;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Logical
    {
        private EVM _evm;
        
        [SetUp]
        public void SetUp()
        {
            _evm = new EVM();
        }

        [Test]
        public void Or()
        {
            var val = _evm.Evaluate("ret true || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void And()
        {
            var val = _evm.Evaluate("ret true && false;");
            Assert.IsFalse(val.IsTruth);
        }
        
        [Test]
        public void NotTrue()
        {
            var val = _evm.Evaluate("ret !true;");
            Assert.IsFalse(val.IsTruth);
        }
        
        [Test]
        public void NotFalse()
        {
            var val = _evm.Evaluate("ret !false;");
            Assert.IsTrue(val.IsTruth);
        }
        
        [Test]
        public void StringIsTruthy()
        {
            var val = _evm.Evaluate("ret \"string\" || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void TableIsTruthy()
        {
            var val = _evm.Evaluate("ret {} || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void ChunkIsTruthy()
        {
            var val = _evm.Evaluate("ret fn() {} || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void NullIsFalsey()
        {
            var val = _evm.Evaluate("ret null && true;");
            Assert.IsFalse(val.IsTruth);
        }
    }
}