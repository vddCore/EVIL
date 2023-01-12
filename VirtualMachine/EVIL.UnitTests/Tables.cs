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
            XAssert.ExistsIn(val, 21.37, out var entry);
            XAssert.AreEqual("jp2gmd", entry);
        }

        [Test]
        public void SetAndUnsetViaNull()
        {
            var val = EVM.Evaluate("t = {}; t[0] = 2; ret t;");
            XAssert.ExistsIn(val, 0, out var entry);
            XAssert.AreEqual(2, entry);

            EVM.Evaluate("t[0] = null;");
            XAssert.DoesNotExistIn(val, 0);
        }

        [Test]
        public void SetWithInitializer()
        {
            var val = EVM.Evaluate("t = { 0 => 1, 1 => 3, 2 => 5 }; ret t;");

            XAssert.ExistsIn(val, 0, out var entry);
            XAssert.AreEqual(1, entry);
            XAssert.ExistsIn(val, 1, out entry);
            XAssert.AreEqual(3, entry);
            XAssert.ExistsIn(val, 2, out entry);
            XAssert.AreEqual(5, entry);
        }
    }
}