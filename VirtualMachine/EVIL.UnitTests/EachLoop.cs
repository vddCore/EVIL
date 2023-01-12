using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class EachLoop : EvmTest
    {
        [Test]
        public void EachLoopCompoundAssignmentProduct()
        {
            var val = EVM.Evaluate(
                "loc table = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };" +
                "loc out = { 0 };" +
                "each (loc v : table) {" +
                "  out[0] += $v;" +
                "}" +
                "ret out[0];"
            );

            XAssert.AreEqual(
                1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9,
                val
            );
        }

        [Test]
        public void EachLoopInFunction()
        {
            var val = EVM.Evaluate(
                "fn max(t) {" +
                "   loc max = 0;" +
                "   each (loc v : t) {" +
                "       if (v > max)" +
                "           max = v;" +
                "   }" +
                "   ret max;" +
                "}" +
                "" +
                "loc x = { 1, 2, 3, 4, 5, 6 };" +
                "ret \"fuck:\" + @max(x);"
            );
            
            XAssert.AreEqual("fuck:6", val);
        }
    }
}