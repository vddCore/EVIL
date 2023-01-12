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

        [Test]
        public void ConcatStringifiedFunctionCallReturn()
        {
            var val = EVM.Evaluate(
                "fn a() { ret 10; } ret \"x\" + @a();"
            );
            
            XAssert.AreEqual("x10", val);
        }
        
        [Test]
        public void ConcatStringifiedFunctionCallReturn2()
        {
            var val = EVM.Evaluate(
                "fn a(v) { ret v; } " +
                "fn b(c) { ret a(c); }" +
                "ret \"x\" + @a(10);"
            );
            
            XAssert.AreEqual("x10", val);
        }
    }
}