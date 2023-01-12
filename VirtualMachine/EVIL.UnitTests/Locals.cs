using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Locals : EvmTest
    {
        [Test]
        public void GlobalScopeLocalIsDefinedInRootChunk()
        {
            var val = EVM.Evaluate("loc x = 10; ret x;");
            
            XAssert.DoesNotExistIn(EVM.GlobalTable, "x");
            XAssert.AreEqual(10, val);
        }
    }
}