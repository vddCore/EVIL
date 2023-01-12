using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Tables : EvmTest
    {       
        [Test]
        public void SetByString()
        {
            var val = EVM.Evaluate("t = {}; t[\"a\"] = 10; ret t;");
            XAssert.ExistsIn(val, "a", out var entry);
            XAssert.AreEqual(10, entry);
        }
        
        [Test]
        public void SetByNumber()
        {
            var val = EVM.Evaluate("t = {}; t[21.37] = \"jp2gmd\"; ret t;");
            XAssert.ExistsIn(val, "a", out var entry);
            XAssert.AreEqual("jp2gmd", entry);
        }
    }
}