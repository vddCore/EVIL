using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Assignments : EvmTest
    {
        [Test]
        public void DirectAssignment()
        {
            var compoundVal = EVM.Evaluate("ret a = 20;");
            var expected = 20;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundAdd()
        {
            var compoundVal = EVM.Evaluate("a = 20; ret a += 30;");
            var expected = 50;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundSubtract()
        {
            var compoundVal = EVM.Evaluate("a = 20; ret a -= 30;");
            var expected = -10;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundDivide()
        {
            var compoundVal = EVM.Evaluate("a = 20; ret a /= 10;");
            var expected = 2;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundMultiply()
        {
            var compoundVal = EVM.Evaluate("a = 20; ret a *= 5;");
            var expected = 100;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundModulo()
        {
            var compoundVal = EVM.Evaluate("a = 23; ret a %= 20;");
            var expected = 3;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundBitwiseAnd()
        {
            var compoundVal = EVM.Evaluate("a = 0xFF; ret a &= 0xF0;");
            var expected = 0xF0;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundBitwiseOr()
        {
            var compoundVal = EVM.Evaluate("a = 0x0F; ret a |= 0xF0;");
            var expected = 0xFF;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundBitwiseXor()
        {
            var compoundVal = EVM.Evaluate("a = 0xFF; ret a ^= 0xF0;");
            var expected = 0xF;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundShiftLeft()
        {
            var compoundVal = EVM.Evaluate("a = 4; ret a <<= 2;");
            var expected = 16;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundShiftRight()
        {
            var compoundVal = EVM.Evaluate("a = 16; ret a >>= 2;");
            var expected = 4;

            XAssert.ExistsIn(EVM.GlobalTable, "a", out var val);
            XAssert.AreEqual(expected, val);
            XAssert.AreEqual(expected, compoundVal);
        }

        [Test]
        public void CompoundAddTableElement()
        {
            var val = EVM.Evaluate(
                "blah = { 0 };" +
                "blah[0] += 10;" +
                "ret blah;"
            );
            XAssert.AreEqual(10, val.Table[0]);
        }
    }
}