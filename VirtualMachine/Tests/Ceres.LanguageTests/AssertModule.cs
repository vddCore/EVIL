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
                .OptionalStringAt(1, $"expected expression to be true, but it was false", out var msg);

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
                .OptionalStringAt(1, $"expected expression to be false, but it was true", out var msg);

            if (boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }

        [RuntimeModuleFunction("is_of_type", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue IsOfType(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var value)
                .ExpectTypeCodeAt(1, out var typeCode)
                .OptionalStringAt(2, $"expected the type of `{value}' to be `{typeCode}', but was `{value.Type}'", out var msg);

            if (value.Type != typeCode)
            {
                throw new TestAssertionException(msg);
            }

            return Nil;
        }
        
        [RuntimeModuleFunction("equal", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue Equal(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var actual)
                .ExpectAnyAt(1, out var expected)
                .OptionalStringAt(2, $"expected expression to be equal to `{expected}', but was `{actual}' instead", out var msg);

            if (expected != actual)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("approx_equal", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue ApproximatelyEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var actual)
                .ExpectNumberAt(1, out var expected)
                .OptionalIntegerAt(2, 1, out var precision)
                .OptionalStringAt(3, $"expected expression to approximately (rounded to {precision} decimal places) equal `{expected}', but was `{actual}' instead", out var msg);

            if (double.Round(expected, (int)precision) != double.Round(actual, (int)precision))
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("not_equal", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue NotEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var actual)
                .ExpectAnyAt(1, out var unexpected)
                .OptionalStringAt(2, $"expected expression to NOT be equal to `{unexpected}', but was `{actual}' instead", out var msg);

            if (unexpected == actual)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
    }
}