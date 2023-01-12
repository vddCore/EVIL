using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class Function : EvmTest
    {
        [Test]
        public void GlobalFunction()
        {
            XAssert.AreEqual(
                21.37,
                EVM.Evaluate(
                    "fn test() { " +
                    "   ret 21 + 0.37;" +
                    "}" +
                    "ret test();"
                )
            );
        }

        [Test]
        public void LocalFunction()
        {
            XAssert.AreEqual(
                12.34,
                EVM.Evaluate(
                    "fn test() {" +
                    "   loc f = fn() {" +
                    "       ret 12.34;" +
                    "   };" +
                    "   ret f();" +
                    "}" +
                    "ret test();"
                )
            );
        }

        [Test]
        public void Closure()
        {
            XAssert.AreEqual(
                5,
                EVM.Evaluate(
                    "fn test() {" +
                    "   loc y = 3;" +
                    "   loc f = fn(x) {" +
                    "       ret x + y++;" +
                    "   };" +
                    "   ret f;" +
                    "} " +
                    "ret test()(2);"
                )
            );
            
            XAssert.AreEqual(
                6.9,
                EVM.Evaluate("ret test()(3.9);")
            );
        }

        [Test]
        public void ParameterModification()
        {
            
        }
    }
}