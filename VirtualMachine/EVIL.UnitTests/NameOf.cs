using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class NameOf : EvmTest
    {
        [Test]
        public void NameOfGlobal()
        {
            XAssert.AreEqual(
                "my_global",
                EVM.Evaluate(
                    "my_global = 20; " +
                    "ret ??my_global; "
                )
            );
        }
        
        [Test]
        public void NameOfNonexistentSymbolIsNullInGlobalContext()
        {
            XAssert.AreEqual(
                DynamicValue.Null,
                EVM.Evaluate(
                    "ret ??my_global; "
                )
            );
        }
        
        [Test]
        public void NameOfNonexistentSymbolIsNullInLocalContext()
        {
            XAssert.AreEqual(
                DynamicValue.Null,
                EVM.Evaluate(
                    "fn func() { ret ??my_global; }" +
                    "ret func();"
                )
            );
        }

        [Test]
        public void NameOfLocal()
        {
            XAssert.AreEqual(
                "local", 
                EVM.Evaluate(
                    "fn func() { loc local = 123; ret ??local; }" +
                    "ret func();"
                )
            );
        }
        
        [Test]
        public void NameOfParameter()
        {
            XAssert.AreEqual(
                "x", 
                EVM.Evaluate(
                    "fn func(x) { ret ??x; }" +
                    "ret func();"
                )
            );
        }
    }
}