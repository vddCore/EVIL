using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public partial class Comparisons
    {
        [Test]
        public void NumberEqualToNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1 == 1;")
            );
        }

        [Test]
        public void NumberNotEqualToNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 2 != 1;")
            );    
        }
        
        [Test]
        public void NumberGreaterThanNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1 > 0.5;")
            );
        }

        [Test]
        public void NumberGreaterThanOrEqualToNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1 >= 1;")
            );
        }

        [Test]
        public void NumberLessThanNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1 < 2;")
            );
        }

        [Test]
        public void NumberLessThanOrEqualToNumber()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 2 <= 2;")
            );
        }
        
        [Test]
        public void NumberNotEqualToString()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1234 != \"string\";")
            );
        }

        [Test]
        public void NumberNotEqualToTable()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1234 != {};")
            );
        }
        
        [Test]
        public void NumberNotEqualToChunk()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1234 != fn(){};")
            );
        }

        [Test]
        public void NumberNotEqualToNull()
        {
            XAssert.IsTrue(
                EVM.Evaluate("ret 1234 != null;")
            );
        }

        [Test]
        public void NumberNotEqualToClrFunction()
        {
            EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);

            XAssert.ExistsIn(EVM.GlobalTable, "clrfunc");
            XAssert.IsTrue(
                EVM.Evaluate("ret 1234 != clrfunc;")
            );
        }
        
        [Test]
        public void NumberLessThanTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 < \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 < fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 < {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret 1 < clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 < null;")
            );
        }
        
        [Test]
        public void NumberLessThanOrEqualToTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 <= \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 <= fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 <= {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret 1 <= clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 <= null;")
            );
        }
        
        [Test]
        public void NumberGreaterThanTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 > \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 > fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 > {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret 1 > clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 > null;")
            );
        }
        
        [Test]
        public void NumberGreaterThanOrEqualToMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 >= \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 >= fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 >= {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret 1 >= clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret 1 >= null;")
            );
        }
    }
}