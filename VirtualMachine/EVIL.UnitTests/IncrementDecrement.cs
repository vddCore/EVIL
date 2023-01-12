using EVIL.ExecutionEngine;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class IncrementDecrement : EvmTest
    {
        [Test]
        public void GlobalPostIncrement()
        {
            EVM.GlobalTable.Set("test",  new(0));
            var val = EVM.Evaluate("ret test++;");

            XAssert.AreEqual(0, val);
            XAssert.AreEqual(1, EVM.GlobalTable["test"]);
        }

        [Test]
        public void GlobalPostDecrement()
        {
            EVM.GlobalTable.Set("test", new(22));
            var val = EVM.Evaluate("ret test--;");

            XAssert.AreEqual(22, val);
            XAssert.AreEqual(21, EVM.GlobalTable["test"]);
        }

        [Test]
        public void LocalPostIncrement()
        {
            var val = EVM.Evaluate(
                "fn test() {" +
                "   loc x = 10;" +
                "   loc rv = x++;" +
                "   ret {x, rv};" +
                "} ret test();"
            ).Table;
            
            XAssert.AreEqual(11, val[0]);
            XAssert.AreEqual(10, val[1]);
        }
        
        [Test]
        public void LocalPostDecrement()
        {
            var val = EVM.Evaluate(
                "fn test() {" +
                "   loc x = 10;" +
                "   loc rv = x--;" +
                "   ret {x, rv};" +
                "} ret test();"
            ).Table;
            
            XAssert.AreEqual(9, val[0]);
            XAssert.AreEqual(10, val[1]);
        }

        [Test]
        public void ExternPostIncrement()
        {
            var val = EVM.Evaluate(
                "fn test() {" +
                "   loc x = 25;" +
                "   ret {" +
                "       fn() {" +
                "           ret x++;" +
                "       }," +
                "       x" +
                "   };" +
                "}" +
                "r = test();" +
                "ret { r[0](), r[1] };"
            ).Table;
            
            XAssert.AreEqual(25, val[0]);
            XAssert.AreEqual(25, val[1]);
        }
        
        [Test]
        public void ExternPostDecrement()
        {
            var val = EVM.Evaluate(
                "fn test() {" +
                "   loc x = 25;" +
                "   ret {" +
                "       fn() {" +
                "           ret x--;" +
                "       }," +
                "       x" +
                "   };" +
                "}" +
                "r = test();" +
                "ret { r[0](), r[1] };"
            ).Table;
            
            XAssert.AreEqual(25, val[0]);
            XAssert.AreEqual(25, val[1]);
        }
    }
}