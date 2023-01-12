using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Conversions : EvmTest
    {       
        [Test]
        public void NumberToString()
        {
            var val = EVM.Evaluate("ret @21.37;");
            XAssert.AreEqual("21.37", val);
        }

        [Test]
        public void StringToNumber()
        {
            var val = EVM.Evaluate("ret $\"21.37\";");
            XAssert.AreEqual(21.37, val);
        }
    }
}