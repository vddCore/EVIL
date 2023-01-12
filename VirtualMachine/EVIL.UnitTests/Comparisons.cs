using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public partial class Comparisons : EvmTest
    {
        [Test]
        public void NullOnlyEverEqualToNull()
        {
            XAssert.IsFalse(
                EVM.Evaluate("ret null == 1;")
            );

            XAssert.IsFalse(
                EVM.Evaluate("ret null == \"a string\";")
            );

            XAssert.IsFalse(
                EVM.Evaluate("ret null == fn(){};")
            );

            XAssert.IsFalse(
                EVM.Evaluate("ret null == {};")
            );

            XAssert.IsTrue(
                EVM.Evaluate("ret null == null;")
            );
        }
    }
}