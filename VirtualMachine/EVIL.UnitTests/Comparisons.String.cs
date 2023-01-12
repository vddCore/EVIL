using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public partial class Comparisons
    {
        [Test]
        public void StringEqualToString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string a\" == \"string a\";")
            );
        }

        [Test]
        public void StringNotEqualToString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string a\" != \"string b\";")
            );
        }
        
        [Test]
        public void StringNotEqualToNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string\" != 1234;")
            );
        }

        [Test]
        public void StringNotEqualToTable()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string\" != {};")
            );
        }
        
        [Test]
        public void StringNotEqualToChunk()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string\" != fn(){};")
            );
        }

        [Test]
        public void StringNotEqualToNull()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string\" != null;")
            );
        }

        [Test]
        public void StringNotEqualToClrFunction()
        {
            EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);

            XAssert.ExistsIn(EVM.GlobalTable, "clrfunc");
            XAssert.IsTrue(
                EVM.Evaluate("ret \"string\" != clrfunc;")
            );
        }
        
        [Test]
        public void StringLessThanString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"this is short\" < \"this is, obviously, longer\";")
            );
        }
        
        [Test]
        public void StringLessThanOrEqualToString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"this is short\" < \"this is, obviously, longer\";")
            );
            
            XAssert.IsTrue(
                EVM.Evaluate("ret \"this is short\" <= \"this is, obviously, longer\";")
            );
        }

        [Test]
        public void StringGreaterThanString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"this string is a bit longer\" > \"than this one\";")
            );
        }
        
        [Test]
        public void StringGreaterThanOrEqualToString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret \"this string is a bit longer\" > \"than this one\";")
            );
            
            XAssert.IsTrue(
                EVM.Evaluate("ret \"these are equal\" >= \"these are equal\";")
            );
        }
        
        [Test]
        public void StringLessThanTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" < 1;")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" < fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" < {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret \"a string\" < clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" < null;")
            );
        }
        
        [Test]
        public void StringLessThanOrEqualToTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" <= 1;")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" <= fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" <= {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret \"a string\" <= clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" <= null;")
            );
        }
        
        [Test]
        public void StringGreaterThanTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" > 1;")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" > fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" > {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret \"a string\" > clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" > null;")
            );
        }
        
        [Test]
        public void StringGreaterThanOrEqualToTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" >= 1;")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" >= fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" >= {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret \"a string\" >= clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret \"a string\" >= null;")
            );
        }
    }
}