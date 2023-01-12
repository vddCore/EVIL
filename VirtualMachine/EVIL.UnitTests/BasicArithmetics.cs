using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class BasicArithmetics
    {
        private EVM _evm;
        
        [SetUp]
        public void SetUp()
        {
            _evm = new EVM();
        }
        
        [Test]
        public void Add()
        {
            var val = _evm.Evaluate("ret 21 + 37;");
            
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21.0 + 37.0, val.Number);
        }

        [Test]
        public void Subtract()
        {
            var val = _evm.Evaluate("ret 21 - 37;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21 - 37, val.Number);
        }

        [Test]
        public void Divide()
        {
            var val = _evm.Evaluate("ret 21 / 37;");
            
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21.0 / 37.0, val.Number);
        }

        [Test]
        public void DivideByZero()
        {
            Assert.Throws<VirtualMachineException>(
                () => _evm.Evaluate("ret 21 / 0;")
            );
        }

        [Test]
        public void Multiply()
        {
            var val = _evm.Evaluate("ret 21 * 37;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21 * 37, val.Number);
        }

        [Test]
        public void Modulo()
        {
            var val = _evm.Evaluate("ret 21 % 37;");
            
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21.0 % 37.0, val.Number);
        }

        [Test]
        public void ShiftLeft()
        {
            var val = _evm.Evaluate("ret 3 << 4;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(3 << 4, val.Number);
        }
        
        [Test]
        public void ShiftRight()
        {
            var val = _evm.Evaluate("ret 16384 >> 2;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(16384 >> 2, val.Number);
        }

        [Test]
        public void BitwiseAnd()
        {
            var val = _evm.Evaluate("ret 21 & 37;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(21 & 37, val.Number);
        }
        
        [Test]
        public void BitwiseOr()
        {
            var val = _evm.Evaluate("ret 5 | 2;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(5 | 2, val.Number);
        }
        
        [Test]
        public void BitwiseXor()
        {
            var val = _evm.Evaluate("ret 9 ^ 3;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(9 ^ 3, val.Number);
        }

        [Test]
        public void BitwiseNot()
        {
            var val = _evm.Evaluate("ret ~96;");
            
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(~96, val.Number);
        }

        [Test]
        public void UnaryMinus()
        {
            var val = _evm.Evaluate("ret -10;");

            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(-10, val.Number);
        }

        [Test]
        public void NestedExpression()
        {
            var val = _evm.Evaluate("ret (2 + 3 * 8 - (2 + 8.3) - 1.8 / 3);");
            
            Assert.AreEqual(DynamicValueType.Number, val.Type);
            Assert.AreEqual(2 + 3 * 8 - (2 + 8.3) - 1.8 / 3, val.Number);
        }
    }
}