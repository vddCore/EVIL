using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Base;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Logical : EvmTest
    {        
        [Test]
        public void Or()
        {
            var val = EVM.Evaluate("ret true || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void And()
        {
            var val = EVM.Evaluate("ret true && false;");
            Assert.IsFalse(val.IsTruth);
        }
        
        [Test]
        public void NotTrue()
        {
            var val = EVM.Evaluate("ret !true;");
            Assert.IsFalse(val.IsTruth);
        }
        
        [Test]
        public void NotFalse()
        {
            var val = EVM.Evaluate("ret !false;");
            Assert.IsTrue(val.IsTruth);
        }
        
        [Test]
        public void StringIsTruthy()
        {
            var val = EVM.Evaluate("ret \"string\" || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void TableIsTruthy()
        {
            var val = EVM.Evaluate("ret {} || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void ChunkIsTruthy()
        {
            var val = EVM.Evaluate("ret fn() {} || false;");
            Assert.IsTrue(val.IsTruth);
        }

        [Test]
        public void NullIsFalsey()
        {
            var val = EVM.Evaluate("ret null && true;");
            Assert.IsFalse(val.IsTruth);
        }

        [Test]
        public void ClrFunctionIsTruthy()
        {
            EVM.GlobalTable["clr"] = new DynamicValue((_,_) => DynamicValue.Null);

            var val = EVM.Evaluate("ret clr;");
            Assert.IsTrue(val.IsTruth);
        }
    }
}