using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.UnitTests.Helper;
using NUnit.Framework;

namespace EVIL.UnitTests
{
    public partial class Comparisons
    {
        [Test]
        public void ChunkEqualToChunk()
        {
            XAssert.IsTrue(
                EVM.Evaluate("fn a(a, b, c, d) {} ret a == a;")
            );
        }
        
        [Test]
        public void ChunkNotEqualToChunk()
        {
            XAssert.IsTrue(
                EVM.Evaluate(
                    "fn a(a, b, c, d) {} " +
                    "fn b(a, b, c) {} " +
                    "ret a != b;"
                )
            );
        }
        
        [Test]
        public void ChunkLessThanTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} < \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} < fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} < {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret fn(){} < clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} < null;")
            );
        }
        
        [Test]
        public void ChunkLessThanOrEqualToTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} <= \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} <= fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} <= {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret fn(){} <= clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} <= null;")
            );
        }
        
        [Test]
        public void ChunkGreaterThanTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} > \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} > fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} > {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret fn(){} > clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} > null;")
            );
        }
        
        [Test]
        public void ChunkGreaterThanOrEqualToTypeMismatches()
        {
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} >= \"string\";")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} >= fn() {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} >= {};")
            );

            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(() =>
            {
                EVM.GlobalTable.Set("clrfunc", (_, _) => DynamicValue.Null);
                EVM.Evaluate("ret fn(){} >= clrfunc;");
            });
            
            XAssert.ThrowsWithInner<VirtualMachineException, TypeComparisonException>(
                () => EVM.Evaluate("ret fn(){} >= null;")
            );
        }
    }
}