using System;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class BasicArithmetics : EvmTest
    {
        [Test]
        public void Add()
        {
            var val = EVM.Evaluate("ret 21 + 37;");
            XAssert.AreEqual(21.0 + 37.0, val);
        }

        [Test]
        public void Subtract()
        {
            var val = EVM.Evaluate("ret 21 - 37;");
            XAssert.AreEqual(21 - 37, val);
        }

        [Test]
        public void Divide()
        {
            var val = EVM.Evaluate("ret 21 / 37;");
            XAssert.AreEqual(21.0 / 37.0, val);
        }

        [Test]
        public void DivideByZeroFails()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, DivideByZeroException>(
                () => EVM.Evaluate("ret 21 / 0;")  
            );
        }

        [Test]
        public void Multiply()
        {
            var val = EVM.Evaluate("ret 21 * 37;");
            XAssert.AreEqual(21 * 37, val);
        }

        [Test]
        public void Modulo()
        {
            var val = EVM.Evaluate("ret 21 % 37;");
            XAssert.AreEqual(21.0 % 37.0, val);
        }

        [Test]
        public void ShiftLeft()
        {
            var val = EVM.Evaluate("ret 3 << 4;");
            XAssert.AreEqual(3 << 4, val);
        }
        
        [Test]
        public void ShiftRight()
        {
            var val = EVM.Evaluate("ret 16384 >> 2;");
            XAssert.AreEqual(16384 >> 2, val);
        }

        [Test]
        public void BitwiseAnd()
        {
            var val = EVM.Evaluate("ret 21 & 37;");
            XAssert.AreEqual(21 & 37, val);
        }
        
        [Test]
        public void BitwiseOr()
        {
            var val = EVM.Evaluate("ret 5 | 2;");
            XAssert.AreEqual(5 | 2, val);
        }
        
        [Test]
        public void BitwiseXor()
        {
            var val = EVM.Evaluate("ret 9 ^ 3;");
            XAssert.AreEqual(9 ^ 3, val);
        }

        [Test]
        public void BitwiseNot()
        {
            var val = EVM.Evaluate("ret ~96;");
            XAssert.AreEqual(~96, val);
        }

        [Test]
        public void UnaryMinus()
        {
            var val = EVM.Evaluate("ret -10;");
            XAssert.AreEqual(-10, val);
        }

        [Test]
        public void NestedExpression()
        {
            var val = EVM.Evaluate("ret (2 + 3 * 8 - (2 + 8.3) - 1.8 / 3);");
            XAssert.AreEqual(2 + 3 * 8 - (2 + 8.3) - 1.8 / 3, val);
        }
    }
}