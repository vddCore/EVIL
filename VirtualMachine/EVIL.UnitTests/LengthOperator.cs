using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class LengthOperator : EvmTest
    {
        [Test]
        public void TableLength()
        {
            var val = EVM.Evaluate("ret #{1, 2, 3};");
            XAssert.AreEqual(3, val);
        }

        [Test]
        public void StringLength()
        {
            var val = EVM.Evaluate("ret #\"7 chars\";");
            XAssert.AreEqual(7, val);
        }

        [Test]
        public void NumberLengthFails()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, UnmeasurableTypeException>(
                () => EVM.Evaluate("ret #123;")
            );
        }

        [Test]
        public void ChunkLengthFails()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, UnmeasurableTypeException>(
                () => EVM.Evaluate("ret #fn(){};")
            );
        }

        [Test]
        public void NullLengthFails()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, UnmeasurableTypeException>(
                () => EVM.Evaluate("ret #null;")
            );
        }
    }
}