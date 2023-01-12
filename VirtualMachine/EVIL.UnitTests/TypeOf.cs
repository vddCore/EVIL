using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Base;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public class TypeOf : EvmTest
    {
        [Test]
        public void TypeOfNumber()
        {
            XAssert.AreEqual(
                "number",
                EVM.Evaluate("ret typeof(21.37);")
            );
        }
        
        [Test]
        public void TypeOfString()
        {
            XAssert.AreEqual(
                "string",
                EVM.Evaluate("ret typeof(\"this is a string\");")
            );
        }
        
        [Test]
        public void TypeOfTable()
        {
            XAssert.AreEqual(
                "table",
                EVM.Evaluate("ret typeof({});")
            );
        }

        [Test]
        public void TypeOfFunction()
        {
            XAssert.AreEqual(
                "function",
                EVM.Evaluate("ret typeof(fn(){});")
            );
        }

        [Test]
        public void TypeOfClrFunction()
        {
            EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
            XAssert.AreEqual(
                "clrfunction",
                EVM.Evaluate("ret typeof(clrfunc);")
            );
        }

        [Test]
        public void TypeOfNull()
        {
            XAssert.AreEqual(
                "null",
                EVM.Evaluate("ret typeof(null);")
            );
        }

        [Test]
        public void TypeOfIndexedValue()
        {
            XAssert.AreEqual(
                "string",
                EVM.Evaluate(
                    "t = {}; " +
                    "t[0] = \"test\"; " +
                    "ret typeof(t[0]);"
                )
            );
        }

        [Test]
        public void TypeOfIdentifierIndexedValue()
        {
            XAssert.AreEqual(
                "string",
                EVM.Evaluate(
                    "t = {}; " +
                    "t.entry = \"test\"; " +
                    "ret typeof(t.entry);"
                )
            );
        }

        [Test]
        public void TypeOfExpression()
        {
            XAssert.AreEqual(
                "number",
                EVM.Evaluate("ret typeof(22 * (3 / 1.3) % 21.37 + (420 - 0.69));")
            );
        }

        [Test]
        public void TypeOfGlobal()
        {
            XAssert.AreEqual(
                "string",
                EVM.Evaluate(
                    "str = \"hello\"; " +
                    "ret typeof(str);"
                )
            );
        }

        [Test]
        public void TypeOfLocal()
        {
            XAssert.AreEqual(
                "number",
                EVM.Evaluate(
                    "fn test() { loc x = 21.37; ret typeof(x); } " +
                    "ret test();"
                )
            );
        }

        [Test]
        public void TypeOfParameter()
        {
            XAssert.AreEqual(
                "number",
                EVM.Evaluate(
                    "fn test(x) { ret typeof(x); }" +
                    "ret test(21.37); "
                )
            );
        }

        [Test]
        public void TypeOfParameterExtern()
        {
            XAssert.AreEqual(
                "string",
                EVM.Evaluate(
                    "fn test(x) { ret fn() { ret typeof(x); }; }" +
                    "ret test(\"hello\")();"
                )
            );
        }
        
        [Test]
        public void TypeOfLocalExtern()
        {
            XAssert.AreEqual(
                "number",
                EVM.Evaluate(
                    "fn test() { " +
                    "   loc x = 21.37; " +
                    "   ret fn() { ret typeof(x); };" +
                    "}" +
                    "ret test()();"
                )
            );
        }
    }
}