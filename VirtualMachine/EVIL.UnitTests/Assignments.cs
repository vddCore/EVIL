using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Assignments
    {
        private EVM _evm;
        
        [SetUp]
        public void SetUp()
        {
            _evm = new EVM();
        }
        
        [Test]
        public void DirectAssignment()
        {
            var compoundVal = _evm.Evaluate("ret a = 20;");
            var expected = 20;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }

        [Test]
        public void CompoundAdd()
        {
            var compoundVal = _evm.Evaluate("a = 20; ret a += 30;");
            var expected = 50;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundSubtract()
        {
            var compoundVal = _evm.Evaluate("a = 20; ret a -= 30;");
            var expected = -10;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundDivide()
        {
            var compoundVal = _evm.Evaluate("a = 20; ret a /= 10;");
            var expected = 2;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundMultiply()
        {
            var compoundVal = _evm.Evaluate("a = 20; ret a *= 5;");
            var expected = 100;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundModulo()
        {
            var compoundVal = _evm.Evaluate("a = 23; ret a %= 20;");
            var expected = 3;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundBitwiseAnd()
        {
            var compoundVal = _evm.Evaluate("a = 0xFF; ret a &= 0xF0;");
            var expected = 0xF0;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundBitwiseOr()
        {
            var compoundVal = _evm.Evaluate("a = 0x0F; ret a |= 0xF0;");
            var expected = 0xFF;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundBitwiseXor()
        {
            var compoundVal = _evm.Evaluate("a = 0xFF; ret a ^= 0xF0;");
            var expected = 0xF;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundShiftLeft()
        {
            var compoundVal = _evm.Evaluate("a = 4; ret a <<= 2;");
            var expected = 16;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
        
        [Test]
        public void CompoundShiftRight()
        {
            var compoundVal = _evm.Evaluate("a = 16; ret a >>= 2;");
            var expected = 4;
            
            Assert.IsTrue(_evm.GlobalTable.IsSet("a"));
            
            var val = _evm.GlobalTable["a"];
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(expected, val.Number);
            
            Assert.AreEqual(DynamicValueType.Number, compoundVal.Type);
            Assert.AreEqual(expected, compoundVal.Number);
        }
    }
}