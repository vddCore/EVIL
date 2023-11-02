using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.LanguageTests
{
    public class AssertModule : RuntimeModule
    {
        public override string FullyQualifiedName => "assert";

        [RuntimeModuleFunction("is_true", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue IsTrue(Fiber _, params DynamicValue[] args)
        {
            args.ExpectBooleanAt(0, out var boolean)
                .OptionalStringAt(1, $"expected `true', actual was `{boolean}'", out var msg);

            if (!boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("is_false", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue IsFalse(Fiber _, params DynamicValue[] args)
        {
            args.ExpectBooleanAt(0, out var boolean)
                .OptionalStringAt(1, $"expected `false', actual was `{boolean}'", out var msg);

            if (boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("equal", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue Equal(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var expected)
                .ExpectAnyAt(1, out var actual)
                .OptionalStringAt(1, $"expected `{expected}' == `{actual}'", out var msg);

            if (expected != actual)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("not_equal", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue NotEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var expected)
                .ExpectAnyAt(1, out var actual)
                .OptionalStringAt(1, $"expected `{expected}' != `{actual}'", out var msg);

            if (expected == actual)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
    }
}